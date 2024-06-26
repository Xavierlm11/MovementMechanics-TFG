using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGrappling : MonoBehaviour
{

    [Header("References")]
    private Player playerMov;
    public Transform cam;
    public Transform gunTip;
    public LayerMask grappableMask;
    public LineRenderer grappleLine;
    public RawImage pointer;
    public Color normalPointerColor = Color.white;
    public Color pointerColor = Color.green;
    private LevelManager levelManager;
    [Header("Grappling")]
    public float grappleDistance;
    public float grappleDelay;
    public float grappleCooldown;
    private float grappleTimer;
    public float shootToY;

    public float pointerCooldown;
    private float pointerTimer;

    [Header("Inputs")]
    private Vector2 inputs;
    public KeyCode grappleKey = KeyCode.Mouse1;
    private Vector3 grapplePoint;
    public bool isGrapppling;

    // Start is called before the first frame update
    void Start()
    {
        playerMov = GetComponent<Player>();
        cam = playerMov.cam.transform;
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pointerTimer <= 0)
        {
            if (Physics.Raycast(cam.position, cam.forward, grappleDistance, grappableMask))
            {
                pointer.color = pointerColor;
            }
            else
            {
                pointer.color = normalPointerColor;
            }
            pointerTimer = pointerCooldown;
        }

        pointerTimer -= Time.deltaTime;

        if (!levelManager.activePanel)
        {
            if (Input.GetKeyDown(grappleKey))
            {
                StartGrapple();
            }
        }
        if (grappleTimer > 0)
        {
            grappleTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (isGrapppling)
        {
            grappleLine.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        if (grappleTimer <= 0)
        {
            isGrapppling = true;
            playerMov.isFreeze = true;


            RaycastHit hit;
            if (Physics.Raycast(cam.position, cam.forward, out hit, grappleDistance, grappableMask))
            {
                grapplePoint = hit.point;
                playerMov.grappleSound = true;
                Invoke(nameof(GrappleAction), grappleDelay);

            }
            else
            {
                grapplePoint = cam.position + cam.forward * grappleDistance;
                Invoke(nameof(StopGrapple), grappleDelay);
            }
            grappleLine.enabled = true;
            grappleLine.SetPosition(1, grapplePoint);
        }
    }

    private void GrappleAction()
    {
        playerMov.isFreeze = false;

        Vector3 lowPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointYPos = grapplePoint.y - lowPoint.y;
        float highPointParabolic = grapplePointYPos + shootToY;

        if (grapplePointYPos < 0)
            highPointParabolic = shootToY;

        playerMov.JumpToPosition(grapplePoint, highPointParabolic);
        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        isGrapppling = false;
        grappleTimer = grappleCooldown;
        grappleLine.enabled = false;
        playerMov.isFreeze = false;
    }

}
