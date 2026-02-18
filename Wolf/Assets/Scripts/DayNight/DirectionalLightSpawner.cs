using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class DirectionalLightSpawner : NetworkBehaviour
{
    [SerializeField] private DayNightCicle dayNightPrefab;
    private DayNightCicle instance;

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (!IsServer) return;

        if (sceneName == "Village")
        {
            var instance = Instantiate(dayNightPrefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
