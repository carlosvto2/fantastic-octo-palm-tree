using UnityEngine;
using TMPro;
public class WolfInventory : RoleInventory
{
    private int vegetablesCountdown;
    private int vegetablesMaxCountdown;
    private int pawsCount;
    private TextMeshProUGUI vegetablesCountdownText;
    private TextMeshProUGUI pawsText;
    private GameObject panel;

    public WolfInventory(TextMeshProUGUI vegCountdownText, TextMeshProUGUI pawsText, GameObject panel, int maxCountdown = 0)
    {
        this.vegetablesCountdownText = vegCountdownText;
        this.pawsText = pawsText;
        this.panel = panel;
        vegetablesMaxCountdown = maxCountdown;
    }

    public override void Initialize()
    {
        vegetablesCountdown = vegetablesMaxCountdown;
        pawsCount = 3;
        panel.SetActive(true);
        UpdateUI();
    }

    public override void UpdateUI()
    {
        vegetablesCountdownText.text = vegetablesCountdown.ToString();
        pawsText.text = pawsCount.ToString();
    }

    public void DecreaseVegetablesCountdown()
    {
        vegetablesCountdown--;
        if (vegetablesCountdown <= 0)
        {
            vegetablesCountdown = vegetablesMaxCountdown;
            IncreasePaws();
        }
        UpdateUI();
    }

    private void IncreasePaws()
    {
        pawsCount++;
    }
}