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

        transform.Translate(new Vector3(0, (float)(Mathf.Sin(Time.time) * 0.02) * Time.deltaTime , 0), Space.World);

        transform.Rotate(0, 0, roationVel * Time.deltaTime *256f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isCollected)
                levelManager.CollectCoin();
            isCollected = true;
            Destroy(gameObject);
        }
    }
}
