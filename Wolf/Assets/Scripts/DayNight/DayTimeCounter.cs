using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class DayTimeCounter : MonoBehaviour
{
    private GameManager GameManager;
    private TextMeshProUGUI Cont;
    private DayNightCicle dayNightCicle;

    public Color dangerColor = Color.red;
    public Color safeColor = Color.green;

    private bool IsNight = false;

    private void Start()
    {
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        while (true)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if(!canvas) yield return new WaitForSeconds(1f);
            
            while (GameObject.Find("TimeText") == null)
                yield return new WaitForSeconds(1f);

            Cont = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
            GameManager = FindFirstObjectByType<GameManager>();
            dayNightCicle = GetComponent<DayNightCicle>();
            break;
        }
    }

    private void Update()
    {
        if(!dayNightCicle || !Cont) return;

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

        // If the day/night changed, inform the GameManager it changed
        if (IsDangerous != IsNight)
        {
            IsNight = IsDangerous;
            GameManager.RaiseDayNightChanged(IsNight);
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