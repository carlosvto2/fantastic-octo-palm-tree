using UnityEngine;
using TMPro;
using Unity.Netcode;

public class InventoryManager : NetworkBehaviour
{
    [Header("Villager")]
    public TextMeshProUGUI VegetablesAmountText;
    private int vegetablesCount = 0;

    [Header("Wolf")]
    public TextMeshProUGUI VegetablesAmountCountdownText;
    public TextMeshProUGUI PawsAmountText;
    [SerializeField] private int pawsCount = 3;
    [SerializeField] private int vegetablesMaxCountdown = 0;
    private int vegetablesCountdown = 0;

    [Header("Visible Panels")]
    public Canvas playerCanvas;
    public GameObject wolfInventoryPanel;
    public GameObject villagerInventoryPanel;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCanvas != null)
            {
                playerCanvas.enabled = false;
            }
            return;
        }
    }

    public void InitializeInventoryUI(Role gameRole)
    {
        if (!IsOwner) return;

        // Deactivate all the panels
        wolfInventoryPanel.SetActive(false);
        villagerInventoryPanel.SetActive(false);

        if (gameRole.name == RoleName.Wolf)
        {
            wolfInventoryPanel.SetActive(true);
            vegetablesCountdown = vegetablesMaxCountdown;
            UpdateVegetablesCountdownUI();
            UpdatePawsUI();
        }
        else if (gameRole.name == RoleName.Villager)
        {
            villagerInventoryPanel.SetActive(true);
            vegetablesCount = 0;
            UpdateVegetablesUI();
        }
    }

    // ------------------------
    // VILLAGER LOGIC
    // ------------------------

    public void IncreaseVegetables()
    {
        vegetablesCount++;
        UpdateVegetablesUI();
    }

    private void UpdateVegetablesUI()
    {
        VegetablesAmountText.text = vegetablesCount.ToString();
    }



    // ------------------------
    // WOLF LOGIC
    // ------------------------
    
    public void DecreaseVegetablesCountdown()
    {
        vegetablesCountdown--;
        if (vegetablesCountdown <= 0)
        {
            vegetablesCountdown = vegetablesMaxCountdown;
            IncreasePaws();
        }
        UpdateVegetablesCountdownUI();
    }

    private void UpdateVegetablesCountdownUI()
    {
        VegetablesAmountCountdownText.text = vegetablesCountdown.ToString();
    }

    private void IncreasePaws()
    {
        pawsCount++;
        UpdatePawsUI();
    }

    private void UpdatePawsUI()
    {
        PawsAmountText.text = pawsCount.ToString();
    }
}
