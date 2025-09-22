using UnityEngine;
using Unity.Netcode;

public class CropPickUp : NetworkBehaviour
{
    private bool isPlayerNearby = false;
    private PlayerController playerController;

    void Update()
    {
        // If the player is close and press E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            CollectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectServerRpc()
    {
        PickUpCropClientRpc();
    }

    [ClientRpc]
    private void PickUpCropClientRpc()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController = other.GetComponent<PlayerController>();
        if (playerController.gameRole == RoleName.Villager)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        playerController = other.GetComponent<PlayerController>();
        if (playerController.gameRole == RoleName.Villager)
        {
            isPlayerNearby = false;
        }
    }
}