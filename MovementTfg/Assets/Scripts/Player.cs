using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    public float currentSpeed;
    public float groundDrag;
    public float moveSpeed = 10f;
    private float speed = 1.0f;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool canJump = true;
    [SerializeField]
    bool canDoubleJump = false;
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

    [SerializeField]
    private Transform orientation;

    private Vector2 inputs;

    private Vector3 moveDirection;

    private Rigidbody rb;

    private Vector3 finalForce;

    public TMP_Text velTextObj; 
    public TMP_Text stateTextObj; 

    public float playerLimit = -10.0f;

    private MovementState movState;

    private PlayerSliding playerSlideCs;

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

        if (inGround )
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

        velTextObj.text="Vel: " + currentSpeed.ToString("0.00");
        stateTextObj.text="State: " + movState.ToString();
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

        if(Input.GetKeyDown(LcrouchKey) && inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if(Input.GetKeyUp(LcrouchKey) || !inGround)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
        //if ((Input.GetKeyDown(LdashKey)|| Input.GetKeyDown(RdashKey)) && canDash )
        //{
        //    canDash = false;
        //    Dash();
        //    Invoke(nameof(ResetDash), dashCooldown);
        //}

    }

    private void StateManager()
    {
         if (Input.GetKey(LcrouchKey)&& inGround )
        {
            movState = MovementState.Crouching;
            speed = crouchSpeed;
            Debug.Log("agachao");
        }
        else if (playerSlideCs.isSliding && inGround)
        {
            movState = MovementState.Sliding;
        }
        else if(inGround)
        {
            movState = MovementState.Walking;
            speed = moveSpeed;

        }
        else if (!inGround)
        {
            movState = MovementState.Air;
        }
        else
        {
            movState = MovementState.Dashing;
        }
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

        if (inGround)
        {
            rb.AddForce(finalForce, ForceMode.Force);
        }
        else if (!inGround)
        {
            rb.AddForce(finalForce * airMultiplier, ForceMode.Force);
        }
    }
    private void VelocityControl() // limits player's velocity
    {
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (vel.magnitude > speed)
        {
            Vector3 limitVel = vel.normalized * speed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }
        currentSpeed = rb.velocity.magnitude;
    }

    private void Jump()
    {
        jumpCount++;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
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
