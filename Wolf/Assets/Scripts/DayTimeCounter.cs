using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DayTimeCounter : MonoBehaviour
{
   public TextMeshProUGUI Cont;
   public int minutes;
   public float seconds;
   public Color adviceColor;
   public int limitMinutes;
    private void Start()
    {
        ContUpdate();
    }
    private void Update()
    {
        seconds += Time.deltaTime;
        if (seconds>= 60){
            seconds=0;
            minutes+=1;
        }
        ContUpdate();
        if (minutes>= limitMinutes){
            Cont.color = adviceColor;
        }

    }
    public void ContUpdate(){
        if(seconds>9.5f){
            Cont.text=minutes.ToString()+":0"+seconds.ToString("f0");
        }
        else{
            Cont.text=minutes.ToString()+":"+seconds.ToString("f0");
        }
        
    }
}
