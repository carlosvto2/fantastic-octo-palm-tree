using UnityEngine;
using System.Collections.Generic;

public class ExteriorVisibility : MonoBehaviour
{
    [Header("Cono de visión")]
    public Transform windowTransform;
    public float viewAngle = 45f;
    public float viewDistance = 10f;
    public string exteriorLayerName = "Exterior"; // Nombre de la capa exterior

    private Renderer[] exteriorRenderers;
    private int exteriorLayer;

    void Start()
    {
        exteriorLayer = LayerMask.NameToLayer(exteriorLayerName);

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<Renderer> list = new List<Renderer>();

        foreach (var obj in allObjects)
        {
            if (obj.layer == exteriorLayer)
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    list.Add(rend);
                }
            }
        }

        exteriorRenderers = list.ToArray();
    }

    public void HideExteriors(bool Hide)
    {
        if (!Hide)
        {
            foreach (var rend in exteriorRenderers)
            {
                rend.enabled = false;
            }
            return;
        }
        else
        {
            foreach (var rend in exteriorRenderers)
            {
                Vector3 dirToObj = rend.transform.position - windowTransform.position;
                float dist = dirToObj.magnitude;

                if (dist <= viewDistance && Vector3.Angle(windowTransform.forward, dirToObj) <= viewAngle / 2f)
                    rend.enabled = true;
                else
                    rend.enabled = false;
            }
        }
    }
}