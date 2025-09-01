using UnityEngine;
using Unity.Netcode;
using System.Linq;

public enum RoleName
{
    Villager,
    Wolf
}

[System.Serializable]
public class Role
{
    public RoleName name;
    public GameObject model;
    public BaseMovement movement;
    public Animator animator;
}


public class PlayerController : NetworkBehaviour
{
    protected Transform cam;                  // Reference to the main camera's transform
    protected Animator animator;             // Reference to the Animator component

    public Role[] roles;
    private Role currentRole;
    public NetworkVariable<RoleName> PlayerRole = new NetworkVariable<RoleName>(
        RoleName.Villager,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private ClientNetworkAnimator networkAnimator;
    private RoleUI CanvasUI;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkAnimator = GetComponent<ClientNetworkAnimator>();
        CanvasUI = GetComponent<RoleUI>();
        
        // Subscribe to NetworkVariable changes on all clients (including Host).
        // We use IsClient instead of IsOwner because we want every client to react
        // to role changes, not just the owner of this PlayerObject.
        if (IsClient)
            PlayerRole.OnValueChanged += OnRoleChanged;

        if (IsOwner)
        {
            CreateCamera();
            SetActiveRole(PlayerRole.Value);
        }
    }
    private void OnDestroy()
    {
        if (IsOwner)
            PlayerRole.OnValueChanged -= OnRoleChanged;
    }

    void Update()
    {
        if (!IsOwner || currentRole == null) return;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (currentRole.movement != null)
        {
            currentRole.movement.Move(input, Camera.main.transform, currentRole.animator);
        }
    }

    // Find the role in the list of roles 
    private Role FindRole(RoleName roleName) =>
        roles.FirstOrDefault(r => r.name == roleName);

    private void SetActiveRole(RoleName roleName)
    {
        foreach (var role in roles)
        {
            bool active = role.name == roleName;
            role.model.SetActive(active);
            if (role.movement != null)
                role.movement.enabled = active;

            if (active)
                currentRole = role;
        }
    }

    void OnRoleChanged(RoleName oldRole, RoleName newRole)
    {
        // Deactivate previous rol
        Role old = FindRole(oldRole);
        if (old != null)
        {
            old.model.SetActive(false);
            if (old.movement != null) old.movement.enabled = false;
        }
        Role role = FindRole(newRole);
        if (role != null)
        {
            // Activate new rol
            role.model.SetActive(true);
            if (role.movement != null) role.movement.enabled = true;


            currentRole = role;
            if (networkAnimator != null)
            {
                networkAnimator.Animator = currentRole.animator;
            }
        }
    }
    public void SetRole(RoleName newRole)
    {
        if (IsServer)
            PlayerRole.Value = newRole;
        else
            SubmitRoleChangeServerRpc(newRole);
    }

    [ServerRpc]
    private void SubmitRoleChangeServerRpc(RoleName newRole)
    {
        PlayerRole.Value = newRole;
    }

    protected virtual void FixedUpdate() { }

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