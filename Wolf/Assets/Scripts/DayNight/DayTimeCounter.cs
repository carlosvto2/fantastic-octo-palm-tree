using TMPro;
using UnityEngine;

public class DayTimeCounter : MonoBehaviour
{
    private TextMeshProUGUI Cont;
    private DayNightCicle dayNightCicle;

    public Color dangerColor = Color.red;
    public Color safeColor = Color.green;

    private void Awake()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Cont = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        dayNightCicle = GetComponent<DayNightCicle>();
    }

    private void Update()
    {
        float currentHour = dayNightCicle.GetCurrentHour();

        if (IsInDangerZone(currentHour))
        {
            Cont.color = dangerColor;
            ShowCountdownTo(dayNightCicle.endDangerHour, currentHour);
        }
        else
        {
            Cont.color = safeColor;
            ShowCountdownTo(dayNightCicle.startDangerHour, currentHour);
        }
    }

    private bool IsInDangerZone(float hour)
    {
        return hour >= dayNightCicle.startDangerHour || hour < dayNightCicle.endDangerHour;
    }

    private void ShowCountdownTo(float targetHour, float currentHour)
    {
        float remainingHours;

        if (currentHour <= targetHour)
            remainingHours = targetHour - currentHour;
        else
            remainingHours = (24f - currentHour) + targetHour;

        // Convert hours to real seconds
        float totalSecondsRemaining = remainingHours * (dayNightCicle.DayDurationInMinutes * 60f / 24f);

        int minutes = Mathf.FloorToInt(totalSecondsRemaining / 60f);
        int seconds = Mathf.FloorToInt(totalSecondsRemaining % 60f);

        Cont.text = $"{minutes:00}:{seconds:00}";
    }
}