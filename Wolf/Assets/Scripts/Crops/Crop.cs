using UnityEngine;
using Unity.Netcode;

public class Crop : NetworkBehaviour
{
    private Collider isPlayerNearby = null;
    private PlayerController playerController;
    public float harvestTime = 1.5f;
    public InteractionButtonUI InteractionButton;


    void Update()
    {
        // If the player is close and press E
        if (isPlayerNearby != null && Input.GetKeyDown(KeyCode.E))
        {
            // if the character is collecting the crop, remove first the interaction button UI
            if (InteractionButton)
                InteractionButton.ShowButtonUI(false, isPlayerNearby, "");

            // Tell the server, this client wants to pick up the vegetable
            CollectServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectServerRpc(ulong clientId)
    {
        StartHarvestClientRpc(clientId);
    }

    public float GetHarvestingTime(){ return harvestTime; }

    [ClientRpc]
    private void StartHarvestClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
    {
        // Only the client that picked up the vegetable, increases the number
        if (NetworkManager.Singleton.LocalClientId == clientId && playerController != null)
        {
            // Notify the player controller must start the harvest for this crop
            if (playerController)
            {
                HarvestingManager Harvesting = playerController.harvestingManager;
                if (Harvesting)
                    Harvesting.BeginHarvest(this);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void FinishHarvestServerRpc()
    {
        FinishHarvestClientRpc(); // Tell all clients to finish harvest (remove crop)
        Destroy(gameObject);
    }

    [ClientRpc]
    public void FinishHarvestClientRpc()
    {
        // Add to inventory
        if (playerController)
        {
            RoleManager playerRoleManager = playerController.roleManager;

            Role gameRole = playerRoleManager.GetGameRole();
            InventoryManager Inventory = playerController.GetComponent<InventoryManager>();
            if (Inventory)
            {
                if (gameRole.name == RoleName.Villager)
                    Inventory.IncreaseVegetables();
                else if(gameRole.name == RoleName.Wolf)
                    Inventory.DecreaseVegetablesCountdown();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController = other.GetComponent<PlayerController>();
        RoleManager playerRoleManager = playerController.roleManager;
        Role currentRole = playerRoleManager.GetCurrentRole();
        if (currentRole.name == RoleName.Wolf)
            return;

        Role gameRole = playerRoleManager.GetGameRole();
        if (gameRole != null)
        {
            isPlayerNearby = other;

            // Show the UI of the interaction button
            if (InteractionButton)
                InteractionButton.ShowButtonUI(true, other, gameRole.harvestText);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isPlayerNearby = null;
        
        if(InteractionButton)
            InteractionButton.ShowButtonUI(false, other, "");
    }
}