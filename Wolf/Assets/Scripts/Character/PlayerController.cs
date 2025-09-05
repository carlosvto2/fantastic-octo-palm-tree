using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Netcode.Components;

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
    protected Transform cam;                  // Reference to the main camera's transform

    public Role[] roles;
    private Role currentRole;
    public NetworkVariable<RoleName> PlayerRole = new NetworkVariable<RoleName>(
        RoleName.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkAnimator networkAnimator;
    private RoleUI CanvasUI;
    private CharacterController controller;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller = GetComponent<CharacterController>();
        CanvasUI = GetComponent<RoleUI>();

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
            }
        }
    }

    // ------------------------
    // Network Role Change
    // ------------------------
    public void SetRole(RoleName newRole)
    {
        if (IsServer) {
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
        RoleUI.Instance?.ShowRole(role.ToString());
    }
}