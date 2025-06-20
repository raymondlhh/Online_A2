using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
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

    public override void OnEnable()
    {
        base.OnEnable();
        
        // Ensure player is spawned when the scene loads
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            // Add a small delay to ensure everything is initialized
            StartCoroutine(SpawnPlayerDelayed());
        }
    }

    private IEnumerator SpawnPlayerDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
        object isAlive;
        if (
            !PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsAlive", out isAlive) ||
            !(bool)isAlive
        )
        {
            SpawnPlayerAtSpawner(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Method to spawn a player at their designated spawner (called when revived)
    public void SpawnPlayerAtSpawner(int playerActorNumber)
    {
        if (playerPrefab != null && playerSpawners.Length >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            int playerIndex = playerActorNumber - 1; // ActorNumber starts at 1
            playerIndex = Mathf.Clamp(playerIndex, 0, playerSpawners.Length - 1);

            Transform spawnPoint = playerSpawners[playerIndex];
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.Log("Place playerPrefab or assign all spawners!");
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

    // Photon callbacks
    public override void OnJoinedRoom()
    {
        // Spawn the local player when they join the room
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Set IsAlive property
            var props = new ExitGames.Client.Photon.Hashtable();
            props["IsAlive"] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            SpawnPlayerAtSpawner(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // Set IsAlive property for new player
        var props = new ExitGames.Client.Photon.Hashtable();
        props["IsAlive"] = true;
        newPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
    }
}
