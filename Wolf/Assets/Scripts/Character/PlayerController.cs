using UnityEngine;
using Unity.Netcode;

public enum Role
{
    Villager,
    Wolf
}

public abstract class PlayerController : NetworkBehaviour
{
    [SerializeField] protected float moveSpeed = 10f;            // Movement speed
    protected Transform cam;                  // Reference to the main camera's transform
    protected Animator animator;             // Reference to the Animator component
    public NetworkVariable<Role> PlayerRole = new NetworkVariable<Role>(
        Role.Villager, // default value
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>(); // Assumes Animator is on a child model

        if (IsOwner)
        {
            CreateCamera();
            PlayerRole.OnValueChanged += OnRoleChanged;
            // initialize the default value
            OnRoleChanged(Role.Villager, PlayerRole.Value);
        }
    }

    protected virtual void OnRoleChanged(Role oldRole, Role newRole)
    {
        FindFirstObjectByType<RoleUI>().ShowRole(newRole.ToString());
    }

    protected virtual void Update() { }

    protected virtual void FixedUpdate() { }

    public void CreateCamera()
    {
        // Create camera and assign player
        cam = Instantiate(Resources.Load<GameObject>("Main Camera")).transform;
        cam.GetComponent<Camera>().enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;

        // Puedes hacer que siga al jugador, por ejemplo:
        cam.GetComponent<CameraFollow>().SetTarget(transform);
    }
    
    public void SetWolf(bool isWolf)
    {
        PlayerRole.Value = isWolf ? Role.Wolf : Role.Villager;
    }
}
