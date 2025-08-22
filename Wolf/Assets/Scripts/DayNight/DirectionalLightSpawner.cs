using UnityEngine;
using Unity.Netcode;

public class DirectionalLightSpawner : NetworkBehaviour
{
    [SerializeField] private DayNightCicle dayNightPrefab;
    private DayNightCicle instance;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Spawn exactly once from the server so all clients get the same object
            instance = Instantiate(dayNightPrefab);
            instance.GetComponent<NetworkObject>().Spawn(true);
            DontDestroyOnLoad(instance.gameObject); // optional: if you change scenes
        }
    }
}
