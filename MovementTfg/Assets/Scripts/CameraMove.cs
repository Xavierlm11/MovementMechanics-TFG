using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    private Transform camPos;
    
    // Update is called once per frame
    void Update()
    {
        transform.position = camPos.position;
    }
}
