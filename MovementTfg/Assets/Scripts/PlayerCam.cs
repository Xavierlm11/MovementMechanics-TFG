using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    Vector2 sensitivity = Vector2.one;

    [SerializeField]
    Vector2 camRotation = Vector2.zero;

    [SerializeField]
    Transform orientation;

    [SerializeField]
    Transform player;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // mouse input
        float inputX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x;
        float inputY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y;

        //Horizntal Rotation
        camRotation.x += inputX;
        //vertical rotation
        camRotation.y -= inputY;
        camRotation.y = Mathf.Clamp(camRotation.y, -90f, 90f);

        transform.rotation = Quaternion.Euler(camRotation.y, camRotation.x,0);
        orientation.rotation = Quaternion.Euler(0, camRotation.x, 0);
       // player.rotation = orientation.rotation;
        //transform.localEulerAngles = Vector3.right * camRotation;

        // Player.
    }
}
