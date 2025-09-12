using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Tooltip("Ángulo de apertura. Usa negativo si quieres que se abra hacia afuera.")]
    public float openAngle = -90f;
    public float openSpeed = 2f;

    [Tooltip("Collider que bloquea físicamente el paso.")]
    public Collider solidCollider;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;
    private GameObject nearbyVillager = null;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        SetColliderState(true); // La puerta empieza cerrada
    }

    void Update()
    {
        if (nearbyVillager != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(ToggleDoor());
            }
        }
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;
        isOpen = !isOpen;

        // Desactiva el collider mientras se abre para evitar bloqueo
        SetColliderState(false);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;

        // Si se ha cerrado, reactiva el collider
        if (!isOpen)
        {
            SetColliderState(true);
        }
    }

    private void SetColliderState(bool active)
    {
        if (solidCollider != null)
            solidCollider.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Villager"))
        {
            nearbyVillager = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyVillager)
        {
            nearbyVillager = null;
        }
    }
}
