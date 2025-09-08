using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider healthBar;

    private void Awake()
    {
        healthBar = GetComponent<Slider>();
    }
    public void ShowHealth()
    {
        foreach (var img in healthBar.GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            img.enabled = true;
        }
    }
    public void SetMaxHealth(int hp)
    {
        if (!healthBar) return;
        healthBar.maxValue = hp;
        healthBar.value = hp;
    }
    public void SetHealth(int hp)
    {
        if (healthBar)
            healthBar.value = hp;
    }
}
