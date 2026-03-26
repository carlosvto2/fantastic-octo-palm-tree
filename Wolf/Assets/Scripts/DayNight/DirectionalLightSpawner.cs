using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class DirectionalLightSpawner : NetworkBehaviour
{
    [SerializeField] private DayNightCicle dayNightPrefab;
    private DayNightCicle instance;

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (!IsServer) return;

        if (sceneName == "Village" || sceneName == "Village_TEST")
        {
            var instance = Instantiate(dayNightPrefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
