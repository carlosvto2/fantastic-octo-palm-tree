using UnityEngine;
using TMPro;
using Unity.Netcode;
public class InventoryManager : NetworkBehaviour
{
    public Canvas playerCanvas;

    [Header("Villager")]
    public TextMeshProUGUI VegetablesAmountText;
    public GameObject villagerInventoryPanel;

    [Header("Witch")]
    public GameObject witchInventoryPanel;

    [Header("Wolf")]
    public TextMeshProUGUI VegetablesAmountCountdownText;
    public TextMeshProUGUI PawsAmountText;
    public GameObject wolfInventoryPanel;
    [SerializeField] private int vegetablesMaxCountdown = 0;

    private RoleInventory currentInventory;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCanvas != null) playerCanvas.enabled = false;
            return;
        }
    }

    public void InitializeInventoryUI(RoleName roleName)
    {
        wolfInventoryPanel.SetActive(false);
        villagerInventoryPanel.SetActive(false);
        witchInventoryPanel.SetActive(false);

        // create the corresponding inventory for the role
        switch (roleName)
        {
            case RoleName.Villager:
                currentInventory = new VillagerInventory(VegetablesAmountText, villagerInventoryPanel);
                break;
            case RoleName.Witch:
                currentInventory = new WitchInventory(VegetablesAmountText, witchInventoryPanel);
                break;
            case RoleName.Wolf:
                currentInventory = new WolfInventory(VegetablesAmountCountdownText, PawsAmountText, wolfInventoryPanel, vegetablesMaxCountdown);
                break;
        }

        currentInventory.Initialize();
    }


    public void IncreaseVegetables()
    {
        if (currentInventory is VillagerInventory villager) villager.IncreaseVegetables();
    }

    public void DecreaseVegetablesCountdown()
    {
        if (currentInventory is WolfInventory wolf) wolf.DecreaseVegetablesCountdown();
    }
}