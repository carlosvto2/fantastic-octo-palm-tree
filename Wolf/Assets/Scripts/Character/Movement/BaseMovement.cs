using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class BaseMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public CharacterController controller;
    protected Animator animator;
    private NetworkAnimator netAnim;
    public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );
    protected NetworkObject parentNetObj;

    // NetworkVariable solo para lectura por los clientes
    public NetworkVariable<Quaternion> ModelRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Server
    );

    protected void Start()
    {
        animator = GetComponent<Animator>();
        netAnim = GetComponent<NetworkAnimator>();
        parentNetObj = transform.parent?.GetComponent<NetworkObject>();
    }

    public void Move(Vector3 input, Transform cam)
    {
        if (cam == null || IsAttacking.Value) return;
        if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
            return;


        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * input.z + camRight * input.x;

        Transform root = transform.root; // root

        if (move.magnitude > 0.1f)
        {
            NetworkObject rootNetObj = GetComponentInParent<NetworkObject>();
            if (rootNetObj.IsOwner)
            {
                Vector3 motion = move.normalized * moveSpeed * Time.deltaTime;
                controller.Move(motion);

                // rotación
                if (motion != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(move);
                    root.rotation = Quaternion.Slerp(root.rotation, targetRotation, Time.deltaTime * 20f);
                    UpdateRotationServerRpc(targetRotation);
                }
            }
            else
            {
                root.rotation = ModelRotation.Value;
            }
        }


        if (animator != null && parentNetObj.IsOwner)
        {
            float speed = move.magnitude;
            animator.SetFloat("Speed", speed > 0.1f ? 1f : 0f);
        }
    }

    // --- ServerRpc to update the rotation ---
    [ServerRpc(RequireOwnership = false)]
    void UpdateRotationServerRpc(Quaternion rotation)
    {
        ModelRotation.Value = rotation;
    }

    public void StartAttack()
    {
        if (IsOwner)
            IsAttacking.Value = true;
    }
    public void EndAttack()
    {
        if (IsOwner)
            IsAttacking.Value = false;
    }
}