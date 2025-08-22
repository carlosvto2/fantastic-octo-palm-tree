using System.Collections;
using TMPro;
using UnityEngine;

public class RoleUI : MonoBehaviour
{
    private TextMeshProUGUI roleText;

    private void Awake()
    {
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

        // Hide Role after some time
        StartCoroutine(HideRoleAfterDelay(2f));
    }

    private IEnumerator HideRoleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (roleText != null)
            roleText.gameObject.SetActive(false);
    }
}
