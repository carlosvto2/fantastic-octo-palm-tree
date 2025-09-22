using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Netcode.Components;
using TMPro;

public enum RoleName
{
    None,
    Villager,
    Wolf
}

[System.Serializable]
public class Role
{
    public RoleName name;
    public GameObject model;
    public BaseMovement movement;
    public float ColliderRadius;
}


public class PlayerController : NetworkBehaviour
{
    protected Transform cam; // Reference to the main camera's transform

    public Role[] roles;
    public RoleName gameRole; // Role of the game for the player
    private Role currentRole;
    public NetworkVariable<RoleName> PlayerRole = new NetworkVariable<RoleName>(
        RoleName.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkAnimator networkAnimator;
    private RoleUI RoleUI;
    private CharacterController controller;

    private DoorInteraction nearbyDoor;
    public GameObject smokeParticlePrefab;

    // Inventory
    InventoryManager Inventory;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller = GetComponent<CharacterController>();
        RoleUI = GetComponent<RoleUI>();
        Inventory = GetComponent<InventoryManager>();

        // Subscribe to NetworkVariable changes on all clients (including Host).
        // We use IsClient instead of IsOwner because we want every client to react
        // to role changes, not just the owner of this PlayerObject.
        if (IsClient)
            PlayerRole.OnValueChanged += OnRoleChanged;

        if (IsOwner)
        {
            CreateCamera();
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

        // Check if any door close when the E button is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (nearbyDoor != null)
            {
                nearbyDoor.ToggleDoorServerRpc();
            }
        }
    }

    // ------------------------
    // Door Interaction
    // ------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Check if the gameobject has the component DoorInteraction
        DoorInteraction door = other.GetComponent<DoorInteraction>();
        if (door != null)
        {
            nearbyDoor = door;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DoorInteraction door = other.GetComponent<DoorInteraction>();
        if (door != null && door == nearbyDoor)
        {
            nearbyDoor = null;
        }
    }

    // ------------------------
    // Roles & Visibility
    // ------------------------
    private Role FindRole(RoleName roleName) => roles.FirstOrDefault(r => r.name == roleName);

    public void SetModelVisible(GameObject model, bool visible)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = visible;
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
            gameRole = roleName;
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
                controller.radius = role.ColliderRadius;

                SetTag();
                SpawnSmokeEffect();
            }
        }
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

    public void SetTag()
    {
        if (currentRole.name == RoleName.Villager)
            gameObject.tag = "Villager";
        else
            gameObject.tag = "Untagged";
    }

    void SpawnSmokeEffect()
    {
        Vector3 spawnPos = transform.position;
        Quaternion randomRot = Random.rotation;

        GameObject particle = Instantiate(smokeParticlePrefab, spawnPos, randomRot);
        Destroy(particle, 1f);
    }

    [ServerRpc]
    private void SubmitRoleChangeServerRpc(RoleName newRole)
    {
        PlayerRole.Value = newRole;
    }

    // ------------------------
    // Camera
    // ------------------------

    public void CreateCamera()
    {
        // Create camera and assign player
        cam = Instantiate(Resources.Load<GameObject>("Main Camera")).transform;
        cam.GetComponent<Camera>().enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;

        // Follow the character
        cam.GetComponent<CameraFollow>().SetTarget(transform);
    }

    [ClientRpc]
    public void ShowRoleScreenClientRpc(RoleName role, ClientRpcParams clientRpcParams = default)
    {
        RoleUI.ShowRole(role.ToString());
        SetGameRole(role);
    }

    public void IncreaseVegetables(int amount)
    {
        if (Inventory)
            Inventory.IncreaseVegetables(amount);
    }
}