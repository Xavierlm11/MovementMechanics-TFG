using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashing : MonoBehaviour
{

    [Header("Info")]
    public Transform orientation;
    private Rigidbody rb;
    private Player playerMov;
    public PlayerCam cam;
    public GameObject speedLines;
    [Header("Dash")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;
    public float maxDashYSpeed;

    [Header("Settings")]
    public bool useCamForward = true;
    public bool canAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    public float dashCooldown;
    private float dashTimer;
    private Vector3 delayedForce;


    public KeyCode dashKey = KeyCode.Mouse0;
    public List<KeyCode> dashKeys = new List<KeyCode> { KeyCode.LeftShift, KeyCode.Mouse0, KeyCode.Q };
    
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
        bool isKeyDown = false;
  
        for (int i = 0; i < dashKeys.Count; i++)
        {
            if (Input.GetKeyDown(dashKeys[i]))
                isKeyDown = true;
        }
        if (/*Input.GetKeyDown(dashKey)*/isKeyDown && playerMov.dashCount > 0 && playerMov.movState != Player.MovementState.Crouching)
        {
            // playerMov.dashCount = 0;
            DashAction();
            speedLines.SetActive(true);
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
    }

    private void DashAction()
    {
        if (dashTimer > 0)
            return;
        else
            dashTimer = dashCooldown;

        playerMov.isDashing = true;

        playerMov.maxYSpeed = maxDashYSpeed;

        Transform forwardTrans;

        if (useCamForward)
            forwardTrans = cam.transform;
        else
            forwardTrans = orientation;

        Vector3 direction = GetDirection(forwardTrans);

        Vector3 finalForce = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
        {
            rb.useGravity = false;
        }
        delayedForce = finalForce;

        Invoke(nameof(WaitForDashing), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }
    private void WaitForDashing()
    {
        if (resetVel)
        {
            rb.velocity = Vector3.zero;
        }

        rb.AddForce(delayedForce, ForceMode.Impulse);
    }
    private void ResetDash()
    {
        playerMov.isDashing = false;
        playerMov.maxYSpeed = 0;
        speedLines.SetActive(false);
        if (disableGravity)
        {
            rb.useGravity = true;
        }
    }
    private Vector3 GetDirection(Transform forwardT)
    {
        Vector3 direction = new Vector3();
        Vector2 inputs;
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        if (canAllDirections)
        {
            direction = forwardT.forward * inputs.y + forwardT.right * inputs.x;
        }
        else
        {
            direction = forwardT.forward;
        }
        if (inputs.x == 0 && inputs.y == 0)
        {
            direction = forwardT.forward;
        }
        return direction.normalized;
    }
}
