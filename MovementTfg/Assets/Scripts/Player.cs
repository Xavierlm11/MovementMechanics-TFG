using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    
    public float currentSpeed;
    public float groundDrag;
    public float moveSpeed = 10f;
    private float speed = 1.0f;
    [SerializeField]
    private float slideSpeed = 1.0f;
    private float aimedMoveSpeed;
    private float maxAimedMoveSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool canJump = true;
    [SerializeField]
    private bool canDoubleJump = false;
    [Tooltip("Quantity of jumps")]
    public int jumps = 1;
    public int jumpCount = 0;
    public bool jumped = false;

    [Header("Crouch")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Dash")]
    public float dashForce;
    public float dashCooldown;
    //public float airMultiplier;
    bool canDash = true;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode LdashKey = KeyCode.LeftShift;
    public KeyCode LcrouchKey = KeyCode.LeftControl;

    [Header("Ground")]
    public float playerHeight = 1.0f;
    public LayerMask whatIsGround;
    public bool inGround;

    [Header("Slope Managment")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool leavingSlope;

    [SerializeField]
    private Transform orientation;

    private Vector2 inputs;

    private Vector3 moveDirection;

    private Rigidbody rb;

    private Vector3 finalForce;


    public float playerLimit = -10.0f;

    public MovementState movState;

    private PlayerSliding playerSlideCs;

    public bool isSliding;

    public TMP_Text stateTextObj;
    public TMP_Text velTextObj;
    public enum MovementState
    {
        Walking,
        Dashing,
        Sliding,
        Crouching,
        Air
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerSlideCs = GetComponent<PlayerSliding>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;

    }

    // Update is called once per frame
    void Update()
    {

        inGround = Physics.Raycast(transform.position, Vector3.down, (playerHeight * 0.5f) + 0.2f, whatIsGround);
        UpdateInputs();
        VelocityControl();
        StateManager();

        if (inGround)
        {
            rb.drag = groundDrag;
            if (jumped)
            {
                jumped = false;
                canDoubleJump = false;
                jumpCount = 0;
            }
        }
        else
        {
            jumped = true;
            rb.drag = 0;
        }

        velTextObj.text = "Vel: " + currentSpeed.ToString("0.00");
        stateTextObj.text = "State: " + movState.ToString();
        //reset position
        if (transform.position.y < playerLimit)
        {
            transform.position = new Vector3(0, 5, 0);
        }
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        transform.rotation = orientation.rotation;
    }

    private void UpdateInputs()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        //jump

        JumpingManage();
        //if ((inputs.x == 0 && inputs.y == 0) || movState == MovementState.Crouching)
            CrouchManager();
        //if ((Input.GetKeyDown(LdashKey)|| Input.GetKeyDown(RdashKey)) && canDash )
        //{
        //    canDash = false;
        //    Dash();
        //    Invoke(nameof(ResetDash), dashCooldown);
        //}

    }

    private void CrouchManager()
    {
        if ((Input.GetKeyDown(LcrouchKey) ) && inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(LcrouchKey) || !inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateManager()
    {
        if (isSliding )
        {
            movState = MovementState.Sliding;
            if(IsOnSlope() && rb.velocity.y <0.1f)
            {
                aimedMoveSpeed = slideSpeed;
            }
            else
            {
                aimedMoveSpeed = speed;
            }
        }
        else if (Input.GetKey(LcrouchKey) && inGround )
        {

            movState = MovementState.Crouching;
            aimedMoveSpeed = crouchSpeed;
            //Debug.Log("agachao");
        }
        else if (inGround)
        {
            movState = MovementState.Walking;
            aimedMoveSpeed = moveSpeed;

        }
        else if (!inGround)
        {
            movState = MovementState.Air;
        }
        //else
        //{
        //    movState = MovementState.Dashing;
        //}
         if(Mathf.Abs(aimedMoveSpeed - maxAimedMoveSpeed)> 4f && speed !=0) // 4 is the speed where it start the smooth change of velocity
        {
            StopAllCoroutines();
            StartCoroutine(LerpSpeed() );
        }
         else
        {
            speed = aimedMoveSpeed;
        }
        maxAimedMoveSpeed = aimedMoveSpeed;
    }

    private IEnumerator LerpSpeed()
    {
        float time = 0;
        float diff = Mathf.Abs(maxAimedMoveSpeed - speed); //differenve
        float startVal = speed;

        while(time < diff)
        {
            speed = Mathf.Lerp(startVal, aimedMoveSpeed, time / diff);
            time+= Time.deltaTime;
            yield return null;
        }
        speed = aimedMoveSpeed;
    }
    private void JumpingManage()
    {
        if (Input.GetKeyDown(jumpKey) && canJump && inGround && !canDoubleJump)
        {
            canJump = false;
            canDoubleJump = true;

            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (canDoubleJump && Input.GetKeyDown(jumpKey)) // si no funka meter otro if debajo
        {

            if (jumpCount >= jumps - 1)
            {
                canDoubleJump = false;
                //jumpCount = 0;
            }
            Jump();

        }
    }

    private void PlayerMovement()//apply force to move
    {
        moveDirection = orientation.forward * inputs.y + orientation.right * inputs.x;

        finalForce = moveDirection.normalized * speed * 10f;


        if (IsOnSlope() && !leavingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * speed * 20f, ForceMode.Force);
        }
        else if (inGround)
        {
            rb.AddForce(finalForce, ForceMode.Force);
        }
        else// if (!inGround) 
        {
            rb.AddForce(finalForce * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !IsOnSlope();

    }
    private void VelocityControl() // limits player's velocity
    {
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (IsOnSlope() && !leavingSlope)
        {
            if (rb.velocity.magnitude > speed)
            {
                rb.velocity = rb.velocity.normalized * speed;
            }
        }
        else
        {
            if (vel.magnitude > speed)
            {
                Vector3 limitVel = vel.normalized * speed;
                rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
            }
        }
        //currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        currentSpeed = vel.magnitude;
    }

    private void Jump()
    {
        leavingSlope = true;

        jumpCount++;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;

        leavingSlope = false;
    }

    public bool IsOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }


    private void Dash()
    {

        rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);

        rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        canDash = true;
    }

}
