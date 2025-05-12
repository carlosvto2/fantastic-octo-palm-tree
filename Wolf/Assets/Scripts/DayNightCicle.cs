using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DayNightCicle : MonoBehaviour
{
    [Range(0.0f, 24f)] public float Hour = 12;
    public Transform Sun;//DirectionalLight
    [SerializeField] public float DayDurationInMinutes = 1;//the norman duration normaly is 24 minutes
    private float SunX;// x position of DirectionalLight

    void Update()
    {
        Hour += Time.deltaTime * (24 / (60 * DayDurationInMinutes));
        if (Hour >= 24)
        {
            Hour = 0;
        }
        sunRotation();

    }

    void sunRotation()
    {
        SunX = 15 * Hour;// 15 * 24h = 360 complete rotation grades
        Sun.localEulerAngles = new Vector3(SunX, 0, 0);
        if (Hour < 6 || Hour > 18)
        {
            Sun.GetComponent<Light>().intensity = 0; //when it's night the sun intensity is 0 (the sun is "off")
        }
        else
        {
            Sun.GetComponent<Light>().intensity = 1;//to put the sun light on
        }

    }
}

