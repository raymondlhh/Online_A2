using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    public Transform[] playerSpawners;

    private bool isCursorLocked = true;

    [Header("Game Timer")]
    [SerializeField] private float gameDuration = 900f; // 15 minutes in seconds
    private float timeRemaining;
    private bool timerIsRunning = false;
    private TextMeshProUGUI timerText;

    [Header("Spawner Visuals")]
    public GameObject[] spawnerVisuals;

    [Header("Revive Areas")]
    public GameObject[] reviveAreas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // // Lock cursor at start
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null && playerSpawners.Length >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
                playerIndex = Mathf.Clamp(playerIndex, 0, playerSpawners.Length - 1);

                Transform spawnPoint = playerSpawners[playerIndex];
                PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.Log("Place playerPrefab or assign all spawners!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCursorLocked;
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                DisplayTime(timeRemaining);
                Time.timeScale = 0f;
            }
        }
    }

    public void StartGameTimer()
    {
        if (timerText == null)
        {
            Debug.LogWarning("Timer Text is not assigned. Timer will not be displayed.");
        }
        timeRemaining = gameDuration;
        timerIsRunning = true;
    }

    public void RegisterTimerText(TextMeshProUGUI text)
    {
        timerText = text;
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timerText == null) return;

        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void HideSpawners()
    {
        if (spawnerVisuals != null)
        {
            foreach (GameObject spawner in spawnerVisuals)
            {
                if (spawner != null)
                {
                    spawner.SetActive(false);
                }
            }
        }
    }
}
