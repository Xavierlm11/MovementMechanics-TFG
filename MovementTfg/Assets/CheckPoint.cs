using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Transform transChild;
    private Player player;
    private void Start()
    {
        transChild = transform.GetChild(0).transform;
        player=GameObject.Find("Player").GetComponent<Player>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.SetCheckpoint(transChild) ;
        }
    }
}
