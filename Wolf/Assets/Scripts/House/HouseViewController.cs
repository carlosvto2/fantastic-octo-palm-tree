using UnityEngine;

public class HouseViewController : MonoBehaviour
{
    [Header("House")]
    public GameObject roof;
    private Transform playerInside;
    private Collider houseInteriorTrigger;
    private ExteriorVisibility exteriorVisibility;

    void Start()
    {
        houseInteriorTrigger = GetComponent<Collider>();
        exteriorVisibility = GetComponent<ExteriorVisibility>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Villager"))
        {
            if (roof != null) roof.active = false;
            if (exteriorVisibility != null) exteriorVisibility.HideExteriors(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Villager"))
        {
            if (roof != null) roof.active = true;
            if (exteriorVisibility != null) exteriorVisibility.HideExteriors(false);
        }
    }
}
