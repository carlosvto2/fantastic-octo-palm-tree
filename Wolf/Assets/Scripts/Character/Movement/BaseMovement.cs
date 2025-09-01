using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class BaseMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public CharacterController controller;

    // NetworkVariable solo para lectura por los clientes
    public NetworkVariable<Quaternion> ModelRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Server
    );

    public void Move(Vector3 input, Transform cam, Animator animator)
    {
        if (cam == null) return;
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
                // apply movement to the root, not to the model
                root.position += move.normalized * moveSpeed * Time.deltaTime;

                // rotation for the root
                Quaternion targetRotation = Quaternion.LookRotation(move);
                root.rotation = Quaternion.Slerp(root.rotation, targetRotation, Time.deltaTime * 20f);

                // send the rotation to apply for the root to the server
                if (IsOwner)
                {
                    UpdateRotationServerRpc(targetRotation);
                }
            }
            else
            {
                // non owner client apply the received rotation
                root.rotation = ModelRotation.Value;
            }
        }
        if (animator != null)
        {
            float speed = move.magnitude;
            animator.SetFloat("Speed", speed > 0.1f ? 1f : 0f);
        }
    }

    // --- ServerRpc to update the rotation ---
    [ServerRpc]
    void UpdateRotationServerRpc(Quaternion rotation)
    {
        ModelRotation.Value = rotation;
    }
}