using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Transform transChild;
    private Player player;
    private AudioSource audioSource;
    private bool isFinished = false;
    private void Start()
    {
        transChild = transform.GetChild(0).GetChild(0).transform;
        player = GameObject.Find("Player").GetComponent<Player>();

        if (transform.CompareTag("Goal")) 
            audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.SetCheckpoint(transChild);

            if (transform.CompareTag("Goal"))
            {
                if (!isFinished)
                {
                    isFinished = true;
                    audioSource.Play();
                }
            }
        }

    }
}
