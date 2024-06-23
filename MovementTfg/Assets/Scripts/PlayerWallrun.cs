using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWallrun : MonoBehaviour
{
    [Header("Wallrun")]
    public LayerMask wallMask;
    public LayerMask groundMask;
    public float wallrunForce;
    public float wallrunJumpUpForce;
    public float wallrunJumpSideForce;
    public float wallrunTime;
    private float wallrunTimer;

    private bool exitWall;
    [SerializeField] private float exitTime;
    private float exitTimer;

    [Header("Inputs")]
    private Vector2 inputs;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Detectors")]
    public float checkDistance;
    public float jumpHeight;
    private RaycastHit lWallHit;//left
    private RaycastHit rWallHit;//right
    private bool isWallL;
    private bool isWallR;

    [Header("Gravity")]
    public bool usingGravity;
    public float gravityCounter;

    [Header("Info")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private Player playerMov;
    public PlayerCam cam;
    public float camTiltAngle = 5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMov = GetComponent<Player>();
        cam = playerMov.cam;
    }

    // Update is called once per frame
    void Update()
    {
        isWallL = Physics.Raycast(transform.position, orientation.right, out lWallHit, checkDistance, wallMask);
        isWallR = Physics.Raycast(transform.position, -orientation.right, out rWallHit, checkDistance, wallMask);
        StateManager();
    }
    private void FixedUpdate()
    {
        if (playerMov.isWallrunning)
        {
            WallrunMovement();
        }
    }

    private void StateManager()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        if ((isWallL || isWallR) && inputs.y > 0 && !GetIsInGround() && !exitWall)
        {
            if (!playerMov.isWallrunning)
            {
                StartWallrun();
            }
            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
                playerMov.jumpedSound = true;
            }
        }
        else if (exitWall)
        {
            if (playerMov.isWallrunning)
            {
                StopWallrun();
            }
            if (exitTimer > 0)
            {
                exitTimer -= Time.deltaTime;

            }
            if (exitTimer >= 0)
            {
                exitWall = false;
            }
        }
        else
        {
            if (playerMov.isWallrunning)
            {
                StopWallrun();
            }
        }
    }
    private bool GetIsInGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, jumpHeight, groundMask);
    }
    private void StartWallrun()
    {
        playerMov.isWallrunning = true;

        if (isWallL) StartCoroutine(cam.LerpTilt(camTiltAngle));
        else StartCoroutine(cam.LerpTilt(-camTiltAngle));

    }
    private void WallrunMovement()
    {
        //  rb.useGravity = usingGravity;
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);


        Vector3 wallNormal;

        if (isWallR) wallNormal = rWallHit.normal;
        else wallNormal = lWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallrunForce, ForceMode.Force);

        if (usingGravity)
        {
            //  rb.AddForce(transform.up*gravityCounter,ForceMode.Force);
        }

    }
    public void StopWallrun()
    {
        playerMov.isWallrunning = false;

        StartCoroutine(cam.LerpTilt(0));
    }

    private void WallJump()
    {
        exitWall = true;
        exitTimer = exitTime;

        Vector3 wallNormal;

        if (isWallR) wallNormal = rWallHit.normal;
        else wallNormal = lWallHit.normal;

        Vector3 finalJumpForce = transform.up * wallrunJumpUpForce + wallNormal * wallrunJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(finalJumpForce, ForceMode.Impulse);

    }

}
