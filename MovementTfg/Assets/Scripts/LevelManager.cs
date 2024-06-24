using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [Header("Items")]
    public List<GameObject> coinsList = new List<GameObject>();
    [SerializeField] private int maxCoins;
    [SerializeField] private int foundCoins = 0;
    [SerializeField] private TMP_Text coinsText;
    public KeyCode respawnKey = KeyCode.R;
    public KeyCode restartKey = KeyCode.F1;
    public Player player;
    public float playerLimit = 40.0f;

    public TMP_Text stateTextObj;
    public TMP_Text velTextObj;

    // Start is called before the first frame update
    void Start()
    {
        GetCoinsOnScene();

    }

    // Update is called once per frame
    void Update()
    {



        if (player.transform.position.y < playerLimit || Input.GetKeyDown(respawnKey))
        {
            player.RespawnPlayer();
        }
        if (Input.GetKeyDown(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void LateUpdate()
    {
        CanvasTextManager();

    }

    private void CanvasTextManager()
    {
        coinsText.text = "coins: " + foundCoins.ToString() + "/" + maxCoins.ToString();

        stateTextObj.text = "State: " + player.movState.ToString();

        velTextObj.text = "Vel: " + Mathf.Round(player.currentSpeed).ToString("0.00");
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
