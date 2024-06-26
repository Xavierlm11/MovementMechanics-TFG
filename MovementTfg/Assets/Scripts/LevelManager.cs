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
    public PlayerCam camSc;

    public TMP_Text stateTextObj;
    public TMP_Text velTextObj;

    public GameObject optionsPanel;
    public GameObject tutorialPanel;
    public TMP_Text sensibilityText;
    public TMP_Text FOVText;
    public bool activePanel = false;
    public bool activeTutorial = false;

    // Start is called before the first frame update
    void Start()
    {
        GetCoinsOnScene();
        optionsPanel.SetActive(activePanel);
    }

    // Update is called once per frame
    void Update()
    {


        if (!activePanel)
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {

            GameOptions();
        } 
        if (Input.GetKeyDown(KeyCode.F2))
        {

            GameTutorial();
        }
        
    }

    private void LateUpdate()
    {
        CanvasTextManager();
        ManageText();
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
    public void ManageText()
    {
        FOVText.text =camSc.cam.fieldOfView.ToString("0.00");
        sensibilityText.text = camSc.generalSensitivity.ToString("0.00");
    }
    public void GameOptions()
    {

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
    public void GameTutorial()
    {

        if (activeTutorial) activeTutorial = false;
        else activeTutorial = true;

        tutorialPanel.SetActive(activeTutorial);

    }
}
