using UnityEngine;
using TMPro;

public class WitchInventory : RoleInventory
{
    private int vegetablesCount;
    private TextMeshProUGUI vegetablesText;
    private GameObject panel;

    public WitchInventory(TextMeshProUGUI text, GameObject panel)
    {
        vegetablesText = text;
        this.panel = panel;
    }

    public override void Initialize()
    {
        vegetablesCount = 0;
        panel.SetActive(true);
        UpdateUI();
    }

    public override void UpdateUI()
    {
        vegetablesText.text = vegetablesCount.ToString();
    }

    public void IncreaseVegetables()
    {
        vegetablesCount++;
        UpdateUI();
    }
}
