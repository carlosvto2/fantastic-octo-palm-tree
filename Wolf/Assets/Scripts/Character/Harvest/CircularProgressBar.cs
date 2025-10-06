using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularProgressBar : MonoBehaviour
{
    private bool isActive = false;
    private float indicatorTimer;
    private float maxIndicatorTimer;

    public Image radialProgressBar;
    public Transform target;
    [SerializeField]
    private TextMeshProUGUI progressBarText;

    public void SetProgressBarText(string textToShow)
    {
        progressBarText.text = textToShow;
    }

    public void UpdateCountdown(float indicatorTimer)
    {
        if (radialProgressBar)
            radialProgressBar.fillAmount = (indicatorTimer / maxIndicatorTimer);
    }

    public void ActivateCountdown(float countdownTime)
    {
        maxIndicatorTimer = countdownTime;
        indicatorTimer = 0;
    }

    private void LateUpdate()
    {
        if (target != null)
            transform.position = target.position;

        transform.forward = Camera.main.transform.forward;
    }
}
