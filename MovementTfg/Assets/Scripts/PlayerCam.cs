using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    private Vector2 sensitivity = Vector2.one;

    [Range(0f, 10f)]
    [SerializeField]
    public float generalSensitivity = 1f;

    [SerializeField]
    private Vector2 camRotation = Vector2.zero;

    [SerializeField]
    private Transform orientation;

    [SerializeField]
    private Transform camHolder;

    [SerializeField]
    private Transform player;

    public Camera cam;

    public float startFov;
    public float actualFov;

    private float maxFovAngle;
    public float fovLerpTime;
    public float tiltLerpTime;

    public GameObject optionsPanel;
    public TMP_Text sensibilityText;
    public TMP_Text FOVText;
    private bool activePanel = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<Camera>();
        startFov = cam.fieldOfView;
        optionsPanel.SetActive(activePanel);
    }

    // Update is called once per frame
    void Update()
    {
        actualFov = cam.fieldOfView;
        // mouse input

        float inputX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x * generalSensitivity;
        float inputY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y * generalSensitivity;

        //Horizntal Rotation
        camRotation.x += inputX;
        //vertical rotation
        camRotation.y -= inputY;
        camRotation.y = Mathf.Clamp(camRotation.y, -90f, 90f);

        //transform.rotation = Quaternion.Euler(camRotation.y, camRotation.x,0);
        camHolder.rotation = Quaternion.Euler(camRotation.y, camRotation.x, 0);
        orientation.rotation = Quaternion.Euler(0, camRotation.x, 0);

        if (Input.GetKeyDown(KeyCode.Escape))
        {

            GameOptions();
        }
        ManageText();
    }

    public IEnumerator LerpFov(float fov)
    {
        float time = 0;
        float duration = fovLerpTime;
        //float diff = Mathf.Abs(aimfov - speed); //differenve
        float startVal = cam.fieldOfView;
        float endVal = fov;


        while (time < duration)
        {
            cam.fieldOfView = Mathf.Lerp(startVal, endVal, time / duration);
            time += Time.deltaTime;


            yield return null;
        }

        cam.fieldOfView = endVal;

    }
    public IEnumerator LerpTilt(float tilt)
    {
        float time = 0;
        float duration = tiltLerpTime;
        //float diff = Mathf.Abs(aimfov - speed); //differenve
        float startVal = cam.transform.localRotation.z;
        float endVal = tilt;

        Debug.Log("Tiltingh");
        while (time < duration)
        {
            cam.transform.localRotation = Quaternion.Euler(cam.transform.rotation.x, cam.transform.rotation.y, Mathf.Lerp(startVal, endVal, time / duration));
            time += Time.deltaTime;


            yield return null;
        }

        cam.transform.localRotation = Quaternion.Euler(cam.transform.rotation.x, cam.transform.rotation.y, endVal);

    }

    public void ChangeSensibility(float sensibility)
    {
        generalSensitivity = sensibility;
    }

    public void GameOptions()
    {

        // activePanel = activePanel != optionsPanel.activeSelf ? true : false;
       // bool v = activePanel != optionsPanel.activeSelf;

        if (activePanel) activePanel = false;
        else activePanel = true;


        optionsPanel.SetActive(activePanel);
        if (activePanel)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    }

    public void ManageText()
    {
        FOVText.text = cam.fieldOfView.ToString("0.00");
        sensibilityText.text = generalSensitivity.ToString("0.00");
    }
}
