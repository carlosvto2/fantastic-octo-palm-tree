using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;

public class NetworkStarter : MonoBehaviour
{
    public GameObject villagerPrefab;
    public GameObject wolfPrefab;
    public Transform spawnPoint;

    public GameObject uiPanel; // Panel with the botons (Canvas UI)
    public int MinimumNumberOfPlayers = 2;
    private List<ulong> connectedClients = new List<ulong>();
    private List<ulong> WolvesClients = new List<ulong>();

    // ROLES
    private bool rolesAssigned = false;
    public event System.Action<bool> OnDayNightChanged;


    private void Start()
    {
        OnDayNightChanged += ToggleWolvesTransformation;
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
            if (isWolf) WolvesClients.Add(clientId); // save the id of the wolf
            SpawnCharacter(clientId, isWolf);
        }
    }

    private void SpawnCharacter(ulong clientId, bool isWolf)
    {
        Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Vector3 spawnPos = spawnPoint.position + offset;

        GameObject characterInstance = Instantiate(villagerPrefab, spawnPos, Quaternion.identity);
        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);


        var PlayerController = characterInstance.GetComponent<PlayerController>();
        if (PlayerController != null)
        {
            PlayerController.PlayerRole.Value = isWolf ? Role.Wolf : Role.Villager;
        }
    }

    public void RaiseDayNightChanged(bool transformToWolf)
    {
        // Only if server Invoke the wolves transformation
        if (!NetworkManager.Singleton.IsServer) return;
        OnDayNightChanged?.Invoke(transformToWolf);
    }

    // Transform the wolves
    public void ToggleWolvesTransformation(bool TransformToWolf)
    {
        foreach (ulong wolfId in WolvesClients)
        {
            // Get the client network of the wolf
            NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[wolfId].PlayerObject;
            if (playerObj != null)
            {
                // Transform the player
                TransformPlayer(playerObj, TransformToWolf);
            }
        }
    }

    public void TransformPlayer(NetworkObject oldPlayerObj, bool toWolf)
    {
        ulong clientId = oldPlayerObj.OwnerClientId; // Get clent id
        Vector3 pos = oldPlayerObj.transform.position;
        Quaternion rot = oldPlayerObj.transform.rotation;

        oldPlayerObj.Despawn(true); // destroy gameobject in all clients

        GameObject prefab = toWolf ? wolfPrefab : villagerPrefab;
        GameObject newCharacter = Instantiate(prefab, pos, rot);
        NetworkObject netObj = newCharacter.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}