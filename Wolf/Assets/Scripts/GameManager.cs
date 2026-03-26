using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private List<ulong> connectedClients = new List<ulong>();
    private List<ulong> wolvesClients = new List<ulong>();
    private List<ulong> witchesClients = new List<ulong>();

    // ROLES
    private bool rolesAssigned = false;
    [Header("Roles")]
    public List<RoleDistribution> roleDistributions = new List<RoleDistribution>();

    public event System.Action<bool> OnDayNightChanged;

    [System.Serializable]
    public class RoleDistribution
    {
        public int playerCount;
        public List<RoleName> roles;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnDayNightChanged += ToggleNightTransformations;

        if (!IsServer) return;

        // Suscribirse al evento de carga de escena
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
    {
        if (rolesAssigned) return;
        rolesAssigned = true;
        
        connectedClients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        AssignRolesAndSpawn();
    }

    // --------------------------------------------------
    // ROLE ASSIGNMENT
    // --------------------------------------------------

    private void AssignRolesAndSpawn()
    {
        int playerCount = connectedClients.Count;

        RoleDistribution distribution =
            roleDistributions.Find(d => d.playerCount == playerCount);

        if (distribution == null)
        {
            return;
        }

        List<RoleName> rolesToAssign = new List<RoleName>(distribution.roles);
        Shuffle(rolesToAssign);

        for (int i = 0; i < connectedClients.Count; i++)
        {
            ulong clientId = connectedClients[i];
            RoleName role = rolesToAssign[i];

            if (role == RoleName.Wolf)
                wolvesClients.Add(clientId);
            else if (role == RoleName.Witch)
                witchesClients.Add(clientId);

            SpawnCharacter(clientId, role, i);
        }
    }

    private void SpawnCharacter(ulong clientId, RoleName role, int spawnIndex)
    {
        Transform spawn = spawnPoints[spawnIndex % spawnPoints.Length];

        GameObject characterInstance =
            Instantiate(playerPrefab, spawn.position, spawn.rotation);

        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);

        var playerController = characterInstance.GetComponent<PlayerController>();

        if (playerController != null)
        {
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };

            playerController.roleManager
                .ShowRoleScreenClientRpc(role, rpcParams);
        }
    }

    // --------------------------------------------------
    // DAY / NIGHT SYSTEM
    // --------------------------------------------------

    public void RaiseDayNightChanged(bool transform)
    {
        if (!IsServer) return;

        OnDayNightChanged?.Invoke(transform);
        ToggleNightTransformations(transform);
    }

    private void ToggleNightTransformations(bool transform)
    {
        ToggleWolvesTransformation(transform);
        ToggleWitchesTransformation(transform);
    }

    private void ToggleWolvesTransformation(bool transform)
    {
        foreach (ulong wolfId in wolvesClients)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(wolfId))
                continue;

            NetworkObject playerObj =
                NetworkManager.Singleton.ConnectedClients[wolfId].PlayerObject;

            if (playerObj == null) continue;

            var controller = playerObj.GetComponent<PlayerController>();
            if (controller == null) continue;

            if (transform)
                controller.roleManager.SetRole(RoleName.Wolf);
            else
                controller.roleManager.SetRole(RoleName.Villager);
        }
    }

    private void ToggleWitchesTransformation(bool transform)
    {
        foreach (ulong witchId in witchesClients)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(witchId))
                continue;

            NetworkObject playerObj =
                NetworkManager.Singleton.ConnectedClients[witchId].PlayerObject;

            if (playerObj == null) continue;

            var controller = playerObj.GetComponent<PlayerController>();
            if (controller == null) continue;

            if (transform)
                controller.roleManager.SetRole(RoleName.Witch);
            else
                controller.roleManager.SetRole(RoleName.Villager);
        }
    }

    // --------------------------------------------------
    // UTILITY
    // --------------------------------------------------

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
