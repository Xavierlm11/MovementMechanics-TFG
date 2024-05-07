using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    Vector2 sensitivity = Vector2.one;

    [Range(0f, 10f)]
    [SerializeField]
    public float generalSensitivity = 1f;

    [SerializeField]
    Vector2 camRotation = Vector2.zero;

    [SerializeField]
    Transform orientation;

    [SerializeField]
    Transform player;

    //private float lastpos = 0;

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
        //if(Mathf.Abs( lastpos-camRotation.x)>=5f )
        //{
        //    Debug.Log("aaaaaaaa");
        //}
        float inputX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x * generalSensitivity;
        float inputY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y * generalSensitivity;

        //Horizntal Rotation
        camRotation.x += inputX;
        //vertical rotation
        camRotation.y -= inputY;
        camRotation.y = Mathf.Clamp(camRotation.y, -90f, 90f);

        transform.rotation = Quaternion.Euler(camRotation.y, camRotation.x,0);
        orientation.rotation = Quaternion.Euler(0, camRotation.x, 0);
       // player.rotation = orientation.rotation;
        //transform.localEulerAngles = Vector3.right * camRotation;
        //lastpos = camRotation.x;
        // Player.
    }
}
