using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI playersJoinFeed;
    public GameObject previewCameraPrefab;
    
    public int playerCount = 0;
    public bool gameHasStarted = false;
    public bool countdownIsRunning = false;
    public bool gamefinished = false;

    private AudioSource backGroundMusic;

    private GameObject restartMenu;

    // Start is called before the first frame update
    void Start()
    {
        backGroundMusic = GetComponent<AudioSource>();
        backGroundMusic.Stop();
        StartScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartScreen()
    {
        Instantiate(previewCameraPrefab);
        playersJoinFeed = GameObject.FindGameObjectWithTag("UIPlayersJoinFeed").GetComponent<TextMeshProUGUI>();
    }

    public void StartGame()
    {
        Destroy(GameObject.FindGameObjectWithTag("PreviewCamera"));

        backGroundMusic.Play();
        //backGroundMusic.enabled = true;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<ThirdPersonMovement>()?.StartCountdown();
        }
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        countdownIsRunning = true;
        var countdown = 3;
        while(countdown >= 0)
        {
            yield return new WaitForSeconds(1);
            countdown--;
        }

        countdownIsRunning = false;
        gameHasStarted = true;
    }

    public void PlayerJoins(string playerName)
    {
        playerCount++;

        playersJoinFeed.text = $"{playerName} has entered the game\n{playersJoinFeed.text}";
    }

    public void FinishedGame(string playerName)
    {
        gamefinished = true;
        foreach (var UIInfoFeed in GameObject.FindGameObjectsWithTag("UIInfoFeed"))
        {
            var infoFeed = UIInfoFeed.GetComponent<TextMeshProUGUI>();
            infoFeed.text = $"{playerName} has finished the game\n{infoFeed.text}";
        }
    }
}
