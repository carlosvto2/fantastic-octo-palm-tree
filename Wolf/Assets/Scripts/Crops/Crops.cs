using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Crops : NetworkBehaviour
{
    public GameObject[] cropsPrefabs = new GameObject[4];

    public override void OnNetworkSpawn()
    {
        Debug.Log("crops network spawn");

        NetworkManager.SceneManager.OnLoadComplete += SpawnCrops;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadComplete -= SpawnCrops;
    }

    private void SpawnCrops(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        Debug.Log("crops SpawnCrops");
        int childCount = transform.childCount;

        if (cropsPrefabs.Length != 4)
        {
            return;
        }

        List<int> usedIndexes = new List<int>();

        for (int i = 0; i < childCount; i++)
        {
            // Every 5 children, get a new random prefab
            if (i % 5 == 0)
            {
                int prefabIndex;
                do
                {
                    prefabIndex = Random.Range(0, cropsPrefabs.Length);
                } while (usedIndexes.Contains(prefabIndex) && usedIndexes.Count < cropsPrefabs.Length);

                if (usedIndexes.Count >= cropsPrefabs.Length)
                    usedIndexes.Clear();

                usedIndexes.Add(prefabIndex);
            }

            int groupPrefabIndex = usedIndexes[usedIndexes.Count - 1];
            GameObject prefabToSpawn = cropsPrefabs[groupPrefabIndex];

            Transform child = transform.GetChild(i);

            GameObject crop = Instantiate(prefabToSpawn, child.position, transform.rotation);

            NetworkObject netObj = crop.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
        }
    }
}