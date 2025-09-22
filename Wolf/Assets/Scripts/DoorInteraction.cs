using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class DoorInteraction : NetworkBehaviour
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

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        SetColliderState(true); // La puerta empieza cerrada
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ToggleDoorRoutine());
        
        ToggleDoorClientRpc();
    }

    [ClientRpc]
    private void ToggleDoorClientRpc() {
        if (IsServer) return;

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ToggleDoorRoutine());
    }


    private IEnumerator ToggleDoorRoutine()
    {
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;
        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private void SetColliderState(bool active)
    {
        if (solidCollider != null)
            solidCollider.enabled = active;
    }
}
