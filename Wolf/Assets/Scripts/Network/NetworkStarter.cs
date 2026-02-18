using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NetworkStarter : MonoBehaviour
{    public GameObject uiPanel; // Panel with the botons (Canvas UI)
    public int MinimumNumberOfPlayers = 2;
    private List<ulong> connectedClients = new List<ulong>();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }


    private void Start()
    {
        // Hide the bottons
        if (uiPanel != null)
            uiPanel.SetActive(false);

        // Event when the client connects (even the local host)
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (!connectedClients.Contains(clientId))
            connectedClients.Add(clientId);

        Debug.Log($"Client connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (connectedClients.Contains(clientId))
            connectedClients.Remove(clientId);

        Debug.Log($"Client disconnected: {clientId}");
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        Debug.Log("IsServer: " + NetworkManager.Singleton.IsServer);
        Debug.Log("IsHost: " + NetworkManager.Singleton.IsHost);
        Debug.Log($"Players connected: {connectedClients.Count}");

        if (connectedClients.Count >= MinimumNumberOfPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "Village",
                LoadSceneMode.Single
            );
        }
        else
        {
            Debug.Log("Not enough players to start.");
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton != null)
        {
            bool shouldShow = !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;
            if (uiPanel != null)
                uiPanel.SetActive(shouldShow);
        }
        else
        {
            if (uiPanel != null)
                uiPanel.SetActive(false);
        }
    }
}