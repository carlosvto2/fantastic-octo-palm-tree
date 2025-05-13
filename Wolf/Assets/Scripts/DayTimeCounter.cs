using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DayTimeCounter : MonoBehaviour
{
    public TextMeshProUGUI Cont;
    private DayNightCicle dayNightCicle = new DayNightCicle();
    public float TotalHour;
    public float limitHour=19;
    public Color adviceColor= Color.red;
    string FormatTime = "";
    public void Start()
    {
        dayNightCicle = GameObject.Find("DayNightCicle").GetComponent<DayNightCicle>();

    }

    private void Update()
    {
        TotalHour = dayNightCicle.Hour;
        if (TotalHour >= limitHour|| TotalHour<6){
            Cont.color = adviceColor;
        }else{
            Cont.color= Color.green;
        }
        dayTime(TotalHour);

    }
    public void dayTime(float Hour)
    {
        int hourFormat = Mathf.FloorToInt(Hour);
        int minutes = Mathf.RoundToInt((Hour - hourFormat) * 60);

        FormatTime = string.Format("{0:D2}:{1:D2}", hourFormat, minutes);
        Cont.text = FormatTime;

    }


}
