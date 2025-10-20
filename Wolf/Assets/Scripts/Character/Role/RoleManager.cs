using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Diagnostics;

public enum RoleName
{
    None,
    Villager,
    Wolf,
    Witch
}

[System.Serializable]
public class Role
{
    public RoleName name;
    public GameObject model;
    public BaseMovement movement;
    public float ColliderRadius;
    public string harvestText; // Text when collecting the crop
    public string harvestTextLoading; // Text when collecting the crop
}

public class RoleManager : NetworkBehaviour
{
    public Role[] roles;
    private Role gameRole; // Role of the game for the player
    private Role currentRole;
    public NetworkVariable<RoleName> PlayerRole = new NetworkVariable<RoleName>(
        RoleName.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private RoleUI RoleUI;

    private PlayerController playerController;
    public GameObject smokeParticlePrefab;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        RoleUI = GetComponent<RoleUI>();
        playerController = GetComponent<PlayerController>();

        // Subscribe to NetworkVariable changes on all clients (including Host).
        // We use IsClient instead of IsOwner because we want every client to react
        // to role changes, not just the owner of this PlayerObject.
        if (IsClient)
            PlayerRole.OnValueChanged += OnRoleChanged;

        if (IsOwner)
        {
            SetActiveRole(RoleName.Villager);
        }
    }

    private void OnDestroy()
    {
        if (IsClient)
            PlayerRole.OnValueChanged -= OnRoleChanged;
    }

    void Update()
    {
        if (!IsOwner || currentRole == null) return;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (currentRole.movement != null)
        {
            currentRole.movement.Move(input, Camera.main.transform);
        }
    }

    public void SetActiveRole(RoleName roleName)
    {
        // Change the role in the server so that all the clients can see it
        if (IsOwner)
            SetRole(roleName);
    }

    public void SetGameRole(RoleName roleName)
    {
        // Change the role in the server so that all the clients can see it
        if (IsOwner)
        {
            foreach (var role in roles)
            {
                if (role.name == roleName)
                    gameRole = role;
            }

            // Get the inventory and initialize it
            InventoryManager Inventory = GetComponent<InventoryManager>();
            if (Inventory)
            {
                Inventory.InitializeInventoryUI(gameRole.name);
            }
        }
    }

    void OnRoleChanged(RoleName oldRole, RoleName newRole)
    {
        // Update the visibility in all the clients
        UpdateRoleVisibilityClientRpc(newRole);
    }

    [ClientRpc]
    private void UpdateRoleVisibilityClientRpc(RoleName activeRole)
    {
        foreach (var role in roles)
        {
            bool visible = role.name == activeRole;
            SetModelVisible(role.model, visible);

            if (role.movement != null)
                role.movement.enabled = visible;

            if (visible)
            {
                currentRole = role;
                playerController.controller.radius = role.ColliderRadius;

                SetTag();
                SpawnSmokeEffect();
            }
        }
    }

    
    void SpawnSmokeEffect()
    {
        Vector3 spawnPos = transform.position;
        Quaternion randomRot = Random.rotation;

        GameObject particle = Instantiate(smokeParticlePrefab, spawnPos, randomRot);
        Destroy(particle, 1f);
    }

    
    public void SetTag()
    {
        if (currentRole.name == RoleName.Villager)
            gameObject.tag = "Villager";
        else
            gameObject.tag = "Untagged";
    }

    // ------------------------
    // Network Role Change
    // ------------------------
    public void SetRole(RoleName newRole)
    {
        if (IsServer)
        {
            PlayerRole.Value = newRole;
        }
        else
            SubmitRoleChangeServerRpc(newRole);
    }


    [ServerRpc]
    private void SubmitRoleChangeServerRpc(RoleName newRole)
    {
        PlayerRole.Value = newRole;
    }

    [ClientRpc]
    public void ShowRoleScreenClientRpc(RoleName role, ClientRpcParams clientRpcParams = default)
    {
        RoleUI.ShowRole(role.ToString());
        SetGameRole(role);
    }
    
    
    // private Role FindRole(RoleName roleName) => roles.FirstOrDefault(r => r.name == roleName);

    public Role GetCurrentRole() { return currentRole; }
    public Role GetGameRole() { return gameRole; }

    public void SetModelVisible(GameObject model, bool visible)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = visible;
        }
    }

}
