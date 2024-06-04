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
    public float wallrunTime;
    private float wallrunTimer;

    [Header("Inputs")]
    private Vector2 inputs;

    [Header("Detectors")]
    public float checkDistance;
    public float jumpHeight;
    private RaycastHit lWallHit;//left
    private RaycastHit rWallHit;//right
    private bool isWallL;
    private bool isWallR;

    [Header("Info")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private Player playerMov;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMov = GetComponent<Player>();
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
        if(playerMov.isWallrunning)
        {
            WallrunMovement();
        }
    }

    private bool GetIsInGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, jumpHeight, groundMask);
    }

    private void StateManager()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        if((isWallL) || (isWallR)&& inputs.y > 0 && GetIsInGround())
        {
            if(!playerMov.isWallrunning)
            {
                StartWallrun();
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

    private void StartWallrun()
    {
        playerMov.isWallrunning = true;
    }
    private void WallrunMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        Vector3 wallNormal;

        if(isWallR) wallNormal = rWallHit.normal;
        else wallNormal = lWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward-wallForward).magnitude > (orientation.forward - -wallForward).magnitude) 
        { 
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward*wallrunForce,ForceMode.Force);
    }
    private void StopWallrun()
    {
        playerMov.isWallrunning = false;
    }


}
