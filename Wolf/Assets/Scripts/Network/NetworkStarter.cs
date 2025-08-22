using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    public GameObject wolfPrefab;
    public GameObject villagerPrefab;
    public Transform spawnPoint;

    public GameObject uiPanel; // Panel with the botons (Canvas UI)

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
                SpawnCharacter(id);
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

    private void SpawnCharacter(ulong clientId)
    {
        GameObject prefabToSpawn = Random.value > 0.5f ? wolfPrefab : villagerPrefab;
        Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Vector3 spawnPos = spawnPoint.position + offset;

        GameObject characterInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}