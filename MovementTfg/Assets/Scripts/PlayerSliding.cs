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
    private bool isCeiling;

    [Header("Slide")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYscale;

    private bool canSlide;
    private bool slidedToEnd;
    public bool afterSlide = false;

    [Header("Inputs")]
    public KeyCode LSlideKey = KeyCode.LeftControl;
    public List<KeyCode> slideKeys = new List<KeyCode> { KeyCode.LeftControl, KeyCode.E, KeyCode.C };
    public Vector2 inputs;
    private LevelManager levelManager;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMov = GetComponent<Player>();
        startYscale = playerObj.localScale.y;
        canSlide = false;
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.y = Input.GetAxisRaw("Vertical");

       
        bool isKeyUp = false;
        bool isKeyPressed = false;
        if (!levelManager.activePanel)
        {
            for (int i = 0; i < slideKeys.Count; i++)
            {
                if (Input.GetKey(slideKeys[i]))
                    isKeyPressed = true;
                if (Input.GetKeyUp(slideKeys[i]))
                    isKeyUp = true;
            }
        }
        if (/*Input.GetKey(LSlideKey)*/isKeyPressed && (inputs.x != 0 || inputs.y > 0) && playerMov.inGround && !canSlide && playerMov.movState != Player.MovementState.Crouching)
        {

            StartSlide();
        }
        if (/*Input.GetKeyUp(LSlideKey)*/isKeyUp && (playerMov.isSliding || slidedToEnd) && !playerMov.isCeil || !playerMov.inGround && playerMov.isSliding)
        {
            slidedToEnd = false;
            canSlide = false;
            afterSlide = false;
            EndSlide();
        }

    }
    private void FixedUpdate()
    {
        if (playerMov.isSliding)
        {
            SlideMovement();
        }
    }

    private void StartSlide()
    {
        canSlide = true;

        playerMov.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        slideTimer = maxSlideTime;
    }

    private void SlideMovement()
    {
        Vector3 inputDir = orientation.forward * inputs.y + orientation.right * inputs.x;

        if (!playerMov.IsOnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(playerMov.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0 && !playerMov.isCeil)
        {
            slidedToEnd = true;
            afterSlide = true;
            EndSlide();
        }
    }

    private void EndSlide()
    {
        playerMov.isSliding = false;
        
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYscale, playerObj.localScale.z);
    }
}
