using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class InteractionButtonUI : MonoBehaviour
{
    public GameObject interactionPanel;
    [SerializeField]
    private TextMeshProUGUI interactionText;

    void Start(){
        if (interactionPanel)
            interactionPanel.SetActive(false);
    }

    public void ShowButtonUI(bool show, Collider Character, string TextToShow)
    {
        // Show/hide the button only for the character that collides
        var netObj = Character.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
        {
            if (interactionPanel)
            {
                // Show the image and the corresponding text
                interactionPanel.SetActive(show);
                interactionText.text = TextToShow;
            }
        }
    }
    private void LateUpdate()
    {
        if(Camera.main)
            transform.forward = Camera.main.transform.forward;
    }
}
