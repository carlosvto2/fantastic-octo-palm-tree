using UnityEngine;
using Unity.Netcode;

public class OutlineHandler : MonoBehaviour
{
    public Material outlineMaterial;

    private Renderer rend;
    private Material[] originalMaterials;
    private PlayerController playerController;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterials = rend.materials;
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController = other.GetComponent<PlayerController>();
        RoleManager playerRoleManager = playerController.roleManager;
        Role currentRole = playerRoleManager.GetCurrentRole();
        if (currentRole.name == RoleName.Wolf)
            return;

        // Show/hide the button only for the character that collides
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
        {
            ApplyOutline(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
        {
            ApplyOutline(false);
        }
    }

    private void ApplyOutline(bool enable)
    {
        if (enable)
        {
            Material[] mats = new Material[originalMaterials.Length + 1];
            for (int i = 0; i < originalMaterials.Length; i++)
                mats[i] = originalMaterials[i];

            mats[mats.Length - 1] = outlineMaterial;
            rend.materials = mats;
        }
        else
        {
            rend.materials = originalMaterials;
        }
    }
}