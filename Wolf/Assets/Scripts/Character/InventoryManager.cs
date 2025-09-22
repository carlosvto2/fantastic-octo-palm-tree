using UnityEngine;
using TMPro;
using Unity.Netcode;

public class InventoryManager : NetworkBehaviour
{
    [Header("Vegetables")]
    public TextMeshProUGUI VegetablesAmountText;
    private int vegetablesCount = 0;
    
    // ------------------------
    // Vegetables
    // ------------------------
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            vegetablesCount = 0;
            UpdateVegetablesUI();
        }
    }

    public void IncreaseVegetables(int amount)
    {
        vegetablesCount += amount;
        UpdateVegetablesUI();
    }

    private void UpdateVegetablesUI()
    {
        VegetablesAmountText.text = vegetablesCount.ToString();
    }
}
