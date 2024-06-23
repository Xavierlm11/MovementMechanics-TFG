using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float currentSpeed = 0;
    public float groundDrag;
    public float moveSpeed = 10f;
    private float speed = 1.0f;
    private float aimedMoveSpeed;
    private float maxAimedMoveSpeed;
    private bool keepMomentum;

    public float speedMultiply;
    public float slopeMultiply;
    private Vector3 velToSet;
    private bool enableMov;

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
    public bool isDashing;
    public float dashSpeed;
    public float dashSpeedMultiplier;
    public int dashCount;
    public float maxYSpeed;

    [Header("Slide")]
    public bool isSliding;
    [SerializeField]
    private float slideSpeed = 1.0f;
    [SerializeField]
    private float addSlideSpeed = 1.0f;

    [Header("Wallrun")]
    public bool isWallrunning;
    [SerializeField]
    private float wallrunSpeed = 1.0f;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode LdashKey = KeyCode.LeftShift;
    public KeyCode LcrouchKey = KeyCode.LeftControl;
    public KeyCode respawnKey = KeyCode.R;
    public KeyCode restartKey = KeyCode.F1;
    public KeyCode changeSceneKey = KeyCode.F2;

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



    public MovementState movState;
    public MovementState lastMovState;

    private PlayerSliding playerSlideSc;


    [Header("Sounds")]
    public AudioSource stepsSource;
    public AudioSource steps2Source;
    public AudioSource coinSource;
    public AudioSource dashSource;
    public AudioSource jumpSource;
    public AudioSource slideSource;
    public AudioSource grappleSource;

    public float runPitch = 1.85f;
    public float walkPitch = 1.85f;
    private float startStepsPitch;
    private bool steped = false;
    public bool jumpedSound = false;
    public bool grappleSound = false;

    [Header("Camera")]
    public PlayerCam cam;
    public float fovAdition = 15f;


    [Header("Checkpoints")]
    public Transform actualCheckpoint;
    [SerializeField]
    private bool isFovChanged = false;
    public bool isFreeze;
    public bool activeGrapple;
    private PlayerGrappling playerGrapplingSc;
    public float playerLimit = -10.0f;

    //public float fovReduction = 80f;

    public TMP_Text stateTextObj;
    public TMP_Text velTextObj;
    public enum MovementState
    {
        Walking,
        Dashing,
        Wallrunning,
        Sliding,
        Crouching,
        Freeze,
        Air
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerSlideSc = GetComponent<PlayerSliding>();
        playerGrapplingSc = GetComponent<PlayerGrappling>();

        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
        startStepsPitch = stepsSource.pitch;

    }

    // Update is called once per frame
    void Update()
    {

        inGround = Physics.Raycast(transform.position, Vector3.down, (playerHeight / 2) + 0.2f, groundMask);

        UpdateInputs();

        VelocityControl();

        StateManager();

        if (inGround && !activeGrapple && movState != MovementState.Dashing)
        {
            rb.drag = groundDrag;
            dashCount = 1;
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

        AudioManager();

        //reset position
        if (transform.position.y < playerLimit || Input.GetKeyDown(respawnKey))
        {
            rb.velocity = Vector3.zero;
            RespawnPlayer();
        }
        if (Input.GetKeyDown(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        //if (Input.GetKeyDown(changeSceneKey))
        //{
        //    if (SceneManager.GetActiveScene().name != "TestScene")
        //        SceneManager.LoadScene("TestScene");
        //    else
        //        SceneManager.LoadScene("LevelTest");
        //}


    }

    private void AudioManager()
    {
        //Active audio while dashing
        if (movState == MovementState.Dashing)
        {
            if (!dashSource.isPlaying)
            {
                dashSource.Play();
            }
        }
        //Active audio while sliding
        if (movState == MovementState.Sliding)
        {
            if (!slideSource.isPlaying)
            {
                slideSource.Play();
            }
        }
        //Active audio while jumping
        if (jumpedSound)
        {
            jumpedSound = false;
            if (!jumpSource.isPlaying)
            {
                jumpSource.Play();
            }
        }
        //Active audio while grappling
        if (grappleSound)
        {
            grappleSound = false;
            if (!grappleSource.isPlaying)
            {
                grappleSource.Play();
            }
        }

        //Active audio while walking or wallrunning
        if ((inputs.x != 0 || inputs.y != 0) && (movState == MovementState.Walking || movState == MovementState.Wallrunning))
        {
            if (!stepsSource.isPlaying && !steps2Source.isPlaying)
            {
                float PitchToUse = startStepsPitch;
                if (movState == MovementState.Wallrunning)
                    PitchToUse = runPitch;


                if (steped)
                {

                    stepsSource.pitch = PitchToUse;
                    stepsSource.Play();
                    steped = false;
                }
                else
                {
                    steps2Source.pitch = PitchToUse;
                    steps2Source.Play();
                    steped = true;

                }


            }
        }
    }

    private void FixedUpdate()
    {

        PlayerMovement();

        transform.rotation = orientation.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMov)
        {
            enableMov = false;
            ResetRestrictions();
            playerGrapplingSc.StopGrapple();
        }
    }

    private void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Coin"))
        {

            if (!coinSource.isPlaying)
                coinSource.Play();
        }
    }

    private void RespawnPlayer()
    {
        transform.position = actualCheckpoint.position;
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
        else if (isFreeze)
        {
            movState = MovementState.Freeze;
            //aimedMoveSpeed = 0;
            //rb.velocity = Vector3.zero;
        }
        else if (isDashing)
        {
            movState = MovementState.Dashing;
            aimedMoveSpeed = dashSpeed;
            speedMultiply = dashSpeedMultiplier;
        }
        else if (isSliding)
        {
            movState = MovementState.Sliding;
            if (IsOnSlope() && rb.velocity.y < 0.1f)
            {
                aimedMoveSpeed = slideSpeed;
            }
            else
            {
                aimedMoveSpeed = moveSpeed + addSlideSpeed;

            }
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
        else  // air state
        {
            movState = MovementState.Air;

            aimedMoveSpeed = moveSpeed;
        }

        if (movState == MovementState.Dashing && dashCount > 0)
        {
            dashCount = 0;
        }
        else if (movState != MovementState.Air && movState != MovementState.Dashing && grappleSound || movState == MovementState.Wallrunning)
        {
            dashCount = 1;
        }

        bool aimedSpeedChanged = aimedMoveSpeed != maxAimedMoveSpeed;

        if (lastMovState == MovementState.Dashing)
        {
            keepMomentum = true;
        }
        if (aimedSpeedChanged)
        {
            if (keepMomentum)
            {
                //StopAllCoroutines();
                StopCoroutine(LerpSpeed());
                StartCoroutine(LerpSpeed());
            }
            else
            {
                //StopAllCoroutines();
                StopCoroutine(LerpSpeed());
                speed = aimedMoveSpeed;
            }
        }

        if (Mathf.Abs(aimedMoveSpeed - maxAimedMoveSpeed) > 4f && speed != 0) // 4 is the speed where it start the smooth change of velocity
        {
            // StopAllCoroutines();
            StopCoroutine(LerpSpeed());
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

        //if (IsOnSlope() && speed >= slideSpeed)
        //      Debug.Log("lerpe 1 : " + speed);



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
        else if (canDoubleJump && Input.GetKeyDown(jumpKey))
        {

            if (jumpCount >= jumps)
            {
                canDoubleJump = false;
                //jumpCount = 0;
            }
            else Jump();

        }
    }

    private void PlayerMovement()//apply force to move
    {
        if (activeGrapple || movState == MovementState.Dashing)
        {
            if (isWallrunning)
                GetComponent<PlayerWallrun>().StopWallrun();
            return;
        }
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

        rb.useGravity = !IsOnSlope();

    }
    private void VelocityControl() // limits player's velocity
    {
        if (activeGrapple)
            return;
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //control velocity on slopes
        if (IsOnSlope() && !leavingSlope)
        {
            if (rb.velocity.magnitude > speed)
            {
                rb.velocity = rb.velocity.normalized * speed;
            }
        }
        // control ground and air velocity
        else
        {
            if (vel.magnitude > speed)
            {
                Vector3 limitVel = vel.normalized * speed;
                rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
            }
        }
        //limits Y velocity
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }

        currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;


        if ((movState == MovementState.Dashing || movState == MovementState.Sliding || grappleSound) && !isFovChanged)
        {
            isFovChanged = true;
            StartCoroutine(cam.LerpFov(cam.startFov + fovAdition));

        }
        else if ((movState == MovementState.Walking || movState == MovementState.Air || movState == MovementState.Wallrunning || movState == MovementState.Crouching) && isFovChanged)
        {

            isFovChanged = false;
            StartCoroutine(cam.LerpFov(cam.startFov));
        }

        if (!IsOnSlope())
            velTextObj.text = "Vel: " + Mathf.Round(vel.magnitude).ToString("0.00");
        else
            velTextObj.text = "Vel: " + Mathf.Round(rb.velocity.magnitude).ToString("0.00");

    }

    private void Jump()
    {
        leavingSlope = true;
        jumpedSound = true;

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

    public void SetCheckpoint(Transform position)
    {
        actualCheckpoint = position;
    }

    public Vector3 CalculateParabolicJump(Vector3 startPos, Vector3 endPos, float maxHeight)
    {
        float gravity = Physics.gravity.y;
        float heightDiff = endPos.y - startPos.y;
        Vector3 posXZDiff = new Vector3(endPos.x - startPos.x, 0f, endPos.z - startPos.z);
        Vector3 velY = Vector3.up * Mathf.Sqrt(-2 * gravity * maxHeight);
        Vector3 velXZ = posXZDiff / (Mathf.Sqrt(-2 * maxHeight / gravity) + Mathf.Sqrt(2 * (heightDiff - maxHeight) / gravity));

        return velXZ + velY;
    }
    public void JumpToPosition(Vector3 finalPos, float jumpHeight)
    {
        activeGrapple = true;
        velToSet = CalculateParabolicJump(transform.position, finalPos, jumpHeight);
        Invoke(nameof(SetVelocity), 0.1f);
        Invoke(nameof(ResetRestrictions), 3f);// in case doesnt reset movement
    }

    private void SetVelocity()
    {
        enableMov = true;
        rb.velocity = velToSet;
    }

    private void ResetRestrictions()
    {
        activeGrapple = false;
    }
}
