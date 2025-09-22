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
            // Tell the server, this client wants to pick up the vegetable
            CollectServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectServerRpc(ulong clientId)
    {
        PickUpCropClientRpc(clientId);
        Destroy(gameObject);
    }

    [ClientRpc]
    private void PickUpCropClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
    {
        // Only the client that picked up the vegetable, increases the number
        if (NetworkManager.Singleton.LocalClientId == clientId && playerController != null)
        {
            playerController.IncreaseVegetables(1);
        }
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