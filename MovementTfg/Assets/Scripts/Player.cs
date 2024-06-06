using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float currentSpeed = 0;
    public float groundDrag;
    public float moveSpeed = 10f;
    private float speed = 1.0f;
    [SerializeField]
    private float slideSpeed = 1.0f;
    [SerializeField]
    private float addSlideSpeed = 1.0f;
    private float aimedMoveSpeed;
    private float maxAimedMoveSpeed;
    [SerializeField]
    private float wallrunSpeed = 1.0f;

    public float speedMultiply;
    public float slopeMultiply;


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
    public LayerMask groundMask;
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
    public bool isWallrunning;

    [SerializeField]
    private GameObject spawner;
    private Transform spawnPoint;


    public TMP_Text stateTextObj;
    public TMP_Text velTextObj;
    public enum MovementState
    {
        Walking,
        Dashing,
        Wallrunning,
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
        spawnPoint = spawner.transform.GetChild(0).transform;
        transform.position = spawnPoint.position;


    }

    // Update is called once per frame
    void Update()
    {

        inGround = Physics.Raycast(transform.position, Vector3.down, (playerHeight / 2) + 0.2f, groundMask);


       
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

       // Debug.Log("speed " + currentSpeed.ToString());

        //reset position
        if (transform.position.y < playerLimit)
        {
            transform.position = spawnPoint.position;
        }
       
    }

    private void FixedUpdate()
    {
        
        PlayerMovement();
        
        transform.rotation = orientation.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {

        }
    }

    private void UpdateInputs()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        //jump

        JumpingManage();
        
        CrouchManager();
        
    }

    private void CrouchManager()
    {
        if ((Input.GetKeyDown(LcrouchKey)) && inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(LcrouchKey) && movState == MovementState.Crouching || !inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateManager()
    {
        if (isWallrunning)
        {
            movState = MovementState.Wallrunning;
            aimedMoveSpeed = wallrunSpeed;
        }
         if (isSliding)
        {
            movState = MovementState.Sliding;
            if (IsOnSlope() && rb.velocity.y < 0.1f)
            {
                aimedMoveSpeed = slideSpeed;
               // Debug.Log("slippppp");
            }
            else
            {
                aimedMoveSpeed = moveSpeed + addSlideSpeed;
                
            }

            if (IsOnSlope() && speed >= slideSpeed)
                Debug.Log("state 2 : " + speed);
        }
        else if (Input.GetKey(LcrouchKey) && inGround)
        {

            movState = MovementState.Crouching;
            aimedMoveSpeed = crouchSpeed;
           
        }
        else if (inGround)
        {
            movState = MovementState.Walking;
            aimedMoveSpeed = moveSpeed;

        }
        else //if (!inGround)
        {
            movState = MovementState.Air;
        }

        if (Mathf.Abs(aimedMoveSpeed - maxAimedMoveSpeed) > 4f && speed != 0) // 4 is the speed where it start the smooth change of velocity
        {
            StopAllCoroutines();
            StartCoroutine(LerpSpeed());
        }
        else
        {
            speed = aimedMoveSpeed;
        }
        maxAimedMoveSpeed = aimedMoveSpeed;

        

        stateTextObj.text = "State: " + movState.ToString();

   
    }

    private IEnumerator LerpSpeed()
    {
        float time = 0;
        float diff = Mathf.Abs(aimedMoveSpeed - speed); //differenve
        float startVal = speed;

        if (IsOnSlope() && speed >= slideSpeed)
            Debug.Log("lerpe 1 : " + speed); 

       

        while (time < diff)
        {
            speed = Mathf.Lerp(startVal, aimedMoveSpeed, time / diff);

            if (IsOnSlope())
            {
                float slopeAng = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngIncrease = 1 + (slopeAng / 90f);
                time += Time.deltaTime * speedMultiply * slopeMultiply * slopeAngIncrease;
            }
            else
            {
                time += Time.deltaTime * speedMultiply;
            }

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

            if (jumpCount >= jumps )
            {
                canDoubleJump = false;
                //jumpCount = 0;
            }
            else Jump();

        }
    }

    private void PlayerMovement()//apply force to move
    {
        moveDirection = orientation.forward * inputs.y + orientation.right * inputs.x;

        finalForce = moveDirection.normalized * speed * 10f;
       

        if (IsOnSlope() && !leavingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * speed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                //to keep the plyer on hte slope and not do weid jumps
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            
        }
        else if (inGround)
        {
            rb.AddForce(finalForce, ForceMode.Force);
        }
        else if (!inGround) 
        {
            rb.AddForce(finalForce * airMultiplier, ForceMode.Force);
        }

       //if(isWallrunning)
       // {

       // rb.useGravity = !IsOnSlope();
       // }
        rb.useGravity = !IsOnSlope();

        if (IsOnSlope() && speed >= slideSpeed)
            Debug.Log("speed on slope 3 : " + speed);

        
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
        currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        //currentSpeed = vel.magnitude;
       
        if(!IsOnSlope())
            velTextObj.text = "Vel: " + Mathf.Round(currentSpeed).ToString("0.00");
        else
            velTextObj.text = "Vel: " + Mathf.Round(rb.velocity.magnitude).ToString("0.00");

        
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
