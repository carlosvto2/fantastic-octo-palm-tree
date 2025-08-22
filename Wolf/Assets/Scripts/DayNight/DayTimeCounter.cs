using TMPro;
using UnityEngine;

public class DayTimeCounter : MonoBehaviour
{
    private NetworkStarter NetworkStarter;
    private TextMeshProUGUI Cont;
    private DayNightCicle dayNightCicle;

    public Color dangerColor = Color.red;
    public Color safeColor = Color.green;

    private bool IsNight = false;

    private void Awake()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Cont = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        // Get the network starter of the scene
        NetworkStarter = FindFirstObjectByType<NetworkStarter>();

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
        bool IsDangerous = hour >= dayNightCicle.startDangerHour || hour < dayNightCicle.endDangerHour;

        // If the day/night changed, inform the NetworkStarter it changed
        if (IsDangerous != IsNight)
        {
            IsNight = IsDangerous;
            NetworkStarter.RaiseDayNightChanged(IsNight);
        }
        return IsDangerous;
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