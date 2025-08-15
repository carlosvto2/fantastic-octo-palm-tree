using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class NetworkStarter : MonoBehaviour
{
    public GameObject wolfPrefab;
    public GameObject villagerPrefab;

    public Transform spawnPoint;

    void OnGUI()
    {
        // Show buttons if no server or client
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Start Host"))
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
                // Initiate host mode
                NetworkManager.Singleton.StartHost();
            }
            if (GUI.Button(new Rect(10, 50, 150, 30), "Start Client"))
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("192.168.2.40", 7777);
                // Initiate client mode
                Debug.Log("test");
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    void Start()
    {
        // Subscribes a function to the event that is triggered when a client connects (including the local Host)
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log(id);
            // Verify the function is executed from server side
            if (NetworkManager.Singleton.IsServer)
            {
                SpawnCharacter(id);
            }
        };
    }

    void SpawnCharacter(ulong clientId)
    {
        GameObject prefabToSpawn = Random.value > 0.5f ? wolfPrefab : villagerPrefab;

        Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Vector3 spawnPos = spawnPoint.position + offset;
        GameObject characterInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();

        // Spawn the character for the corresponding client
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}