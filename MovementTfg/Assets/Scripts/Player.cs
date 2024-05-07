using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 1.0f;
    public float currentSpeed;
    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool canJump = true;
    [SerializeField]
    bool canDoubleJump = false;
    [Tooltip("Cuantity of jumps")]
    public int jumps = 1;
    public int jumpCount = 0;
    public bool jumped = false;

    [Header("Dash")]
    public float dashForce;
    public float dashCooldown;
    //public float airMultiplier;
    bool canDash = true;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode LdashKey = KeyCode.LeftShift;
    public KeyCode RdashKey = KeyCode.RightShift;

    [Header("Ground")]
    public float playerHeight = 1.0f;
    public LayerMask whatIsGround;
    public bool inGround;

    [SerializeField]
    private Transform orientation;

    Vector2 inputs;

    Vector3 moveDirection;

    Rigidbody rb;

    Vector3 finalForce;

    public float playerLimit = -10.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {

        inGround = Physics.Raycast(transform.position, Vector3.down, (playerHeight * 0.5f) + 0.2f, whatIsGround);
        UpdateInputs();

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

        VelocityControl();

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

        if ((Input.GetKeyDown(LdashKey)|| Input.GetKeyDown(RdashKey)) && canDash )
        {
            canDash = false;
            Dash();
            Invoke(nameof(ResetDash), dashCooldown);
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
