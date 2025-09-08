using System.Collections;
using TMPro;
using UnityEngine;

public class RoleUI : MonoBehaviour
{
    public static RoleUI Instance;   // 🔑 Singleton accesible globally
    private TextMeshProUGUI roleText;

    private void Awake()
    {
        // Configure Singleton
        if (Instance == null) 
            Instance = this;
        else
        {
            return;
        }

        // Search the Roletext inside of the canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            roleText = canvas.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
            if (roleText != null)
                roleText.gameObject.SetActive(false);
        }
    }

    public void ShowRole(string role)
    {
        if (!roleText) return;

        roleText.gameObject.SetActive(true);
        roleText.text = $"You are a {role}";

        StartCoroutine(HideRoleAfterDelay(5f));
    }

    private IEnumerator HideRoleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (roleText != null)
            roleText.gameObject.SetActive(false);
    }
}