using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{

    [Header("Info")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private Player playerMov;

    [Header("Slide")]
    public float maxSlideTime;
    private float minSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYscale;
    public bool isSliding;

    [Header("Inputs")]
    public KeyCode LSlideKey = KeyCode.LeftControl;
    private Vector2 inputs;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMov = GetComponent<Player>();
        startYscale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(LSlideKey) && (inputs.x != 0 || inputs.y != 0))
        {
            StartSlide();
        }
        if (Input.GetKeyUp(LSlideKey) && isSliding)
        {
            EndSlide();
        }

    }
    private void FixedUpdate()
    {
        if (isSliding)
        {
            SlideMovement();
        }
    }

    private void StartSlide()
    {
        isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        slideTimer = maxSlideTime;
    }

    private void SlideMovement()
    {
        Vector3 inputDir = orientation.forward * inputs.y + orientation.right * inputs.x;
        rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);
        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0)
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYscale, playerObj.localScale.z);
    }
}
