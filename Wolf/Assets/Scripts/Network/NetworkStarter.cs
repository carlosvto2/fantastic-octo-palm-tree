using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;

public class NetworkStarter : MonoBehaviour
{
    public GameObject wolfPrefab;
    public GameObject villagerPrefab;
    public Transform spawnPoint;

    public GameObject uiPanel; // Panel with the botons (Canvas UI)
    public int MinimumNumberOfPlayers = 2;
    private List<ulong> connectedClients = new List<ulong>();

    // ROLES
    private bool rolesAssigned = false;


    private void Start()
    {
        // Hide the bottons
        if (uiPanel != null)
            uiPanel.SetActive(false);

        // Event when the client connects (even the local host)
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            // Debug.Log($"Cliente conectado: {id}");
            if (NetworkManager.Singleton.IsServer)
            {
                // add client to the connected clients
                connectedClients.Add(id);

                // Execute when all player connected
                if (connectedClients.Count == MinimumNumberOfPlayers && !rolesAssigned)
                {
                    AssignRolesAndSpawn();
                    rolesAssigned = true;
                }
            }
        };
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

    public void StartHost()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
        NetworkManager.Singleton.StartClient();
    }

    private void AssignRolesAndSpawn()
    {
        // Choose randomly the roles
        int wolfIndex = Random.Range(0, connectedClients.Count);
        ulong wolfId = connectedClients[wolfIndex];

        for (int i = 0; i < connectedClients.Count; i++)
        {
            ulong clientId = connectedClients[i];
            bool isWolf = clientId == wolfId;
            SpawnCharacter(clientId, isWolf);
        }
    }

    private void SpawnCharacter(ulong clientId, bool isWolf)
    {
        Role assignedRole = isWolf ? Role.Wolf : Role.Villager;

        GameObject prefabToSpawn = assignedRole == Role.Wolf ? wolfPrefab : villagerPrefab;

        Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Vector3 spawnPos = spawnPoint.position + offset;

        GameObject characterInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
        

        var PlayerController = characterInstance.GetComponent<PlayerController>();
        if (PlayerController != null)
        {
            PlayerController.PlayerRole.Value = assignedRole;
        }
    }
}