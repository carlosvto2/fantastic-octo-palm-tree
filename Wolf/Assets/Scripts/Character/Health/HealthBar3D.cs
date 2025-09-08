using UnityEngine;
using UnityEngine.UI;

public class HealthBar3D : MonoBehaviour
{
    private Slider healthBar;
    public Transform target;

    void Awake()
    {
        healthBar = GetComponentInChildren<Slider>();
    }

    public void SetMaxHealth(int hp)
    {
        if (!healthBar) return;
        healthBar.maxValue = hp;
        healthBar.value = hp;
    }

    public void SetHealth(int hp)
    {
        if (!healthBar) return;
        healthBar.value = hp;
    }

    private void LateUpdate()
    {
        if (target != null)
            transform.position = target.position;

        transform.forward = Camera.main.transform.forward;
    }
}