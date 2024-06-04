using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Range(-1.0f, 1.0f)]
    public float roationVel = 0.5f;

    private LevelManager levelManager;
    private bool isCollected = false;
    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position= new Vector3 (transform.position.x,(float)(Mathf.Sin( Time.time )*0.0002)+transform.position.y, transform.position.z);
        //transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + 1);
        transform.Rotate(0, 0, roationVel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(!isCollected)
                levelManager.CollectCoin();
            isCollected = true;
            Destroy(gameObject);
        }
    }
}
