using System.Collections;
using TMPro;
using UnityEngine;

public class RoleUI : MonoBehaviour
{
    public TextMeshProUGUI roleText;

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