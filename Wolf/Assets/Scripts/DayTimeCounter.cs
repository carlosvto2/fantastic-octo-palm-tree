using TMPro;
using UnityEngine;

public class DayTimeCounter : MonoBehaviour
{
    public TextMeshProUGUI Cont;
    private DayNightCicle dayNightCicle;

    public float startDangerHour = 19f; // Hour when wolf time starts (7 PM)
    public float endDangerHour = 6f;    // Hour when wolf time ends (6 AM)

    public Color dangerColor = Color.red;
    public Color safeColor = Color.green;

    private void Start()
    {
        
        dayNightCicle = GameObject.Find("DayNightCicle").GetComponent<DayNightCicle>();
    }

    private void Update()
    {
        if (dayNightCicle == null) return;

        float currentHour = dayNightCicle.Hour;

        if (IsInDangerZone(currentHour))
        {
            // During danger time (from 19:00 to 6:00) – show countdown to 6:00 AM
            Cont.color = dangerColor;
            ShowCountdownTo(endDangerHour, currentHour);
        }
        else
        {
            // During safe time (from 6:00 to 19:00) – show countdown to 7:00 PM
            Cont.color = safeColor;
            ShowCountdownTo(startDangerHour, currentHour);
        }
    }

    
    private bool IsInDangerZone(float hour)
    {
        return hour >= startDangerHour || hour < endDangerHour;
    }

  
    private void ShowCountdownTo(float targetHour, float currentHour)
    {
        float remainingHours;

        if (currentHour <= targetHour)
        {
            
            remainingHours = targetHour - currentHour;
        }
        else
        {
            
            remainingHours = (24f - currentHour) + targetHour;
        }

        int hours = Mathf.FloorToInt(remainingHours);
        int minutes = Mathf.FloorToInt((remainingHours - hours) * 60);

        Cont.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
