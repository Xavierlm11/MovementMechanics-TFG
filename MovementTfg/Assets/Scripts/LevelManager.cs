using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [Header("Items")]
    public List<GameObject> coinsList = new List<GameObject>();
    [SerializeField] private int maxCoins;
    [SerializeField] private int foundCoins = 0;
    [SerializeField] private TMP_Text  coinsText;

    // Start is called before the first frame update
    void Start()
    {
        GetCoinsOnScene();
    }

    // Update is called once per frame
    void Update()
    {

        coinsText.text= "coins: "+ foundCoins.ToString() + "/"+maxCoins.ToString();
    }

    private void GetCoinsOnScene()
    {
        coinsList.Clear();
        coinsList.AddRange(GameObject.FindGameObjectsWithTag("Coin"));
        maxCoins = coinsList.Count;
        foundCoins = 0;
    }
    public void CollectCoin()
    {
        foundCoins++;
    }
}
