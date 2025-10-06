using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class DoorInteraction : NetworkBehaviour
{
    [Tooltip("Ángulo de apertura. Usa negativo si quieres que se abra hacia afuera.")]
    public float openAngle = -90f;
    public float openSpeed = 2f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;
    public InteractionButtonUI InteractionButtonInside;
    public InteractionButtonUI InteractionButtonOutside;
    private Collider isPlayerNearby = null;
    private PlayerController playerController;
    public string OpenDoorText;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        if (isPlayerNearby == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (InteractionButtonInside)
                InteractionButtonInside.ShowButtonUI(false, isPlayerNearby, "");
            if (InteractionButtonOutside)
                InteractionButtonOutside.ShowButtonUI(false, isPlayerNearby, "");

            ToggleDoorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ToggleDoorRoutine());

        ToggleDoorClientRpc();
    }

    [ClientRpc]
    private void ToggleDoorClientRpc()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (!InteractionButtonOutside || !InteractionButtonOutside) return;

        playerController = other.GetComponent<PlayerController>();
        RoleManager playerRoleManager = playerController.roleManager;
        Role currentRole = playerRoleManager.GetCurrentRole();
        if (currentRole.name == RoleName.Wolf)
            return;

        isPlayerNearby = other;
        Vector3 dir = (other.transform.position - transform.position).normalized;
            
        // Escalar product to know the side of the door
        float dot = Vector3.Dot(transform.forward, dir);

        if (dot > 0)
        {
            InteractionButtonOutside.ShowButtonUI(false, other, "");
            InteractionButtonInside.ShowButtonUI(true, other, OpenDoorText);
        }
        else
        {
            InteractionButtonOutside.ShowButtonUI(true, other, OpenDoorText);
            InteractionButtonInside.ShowButtonUI(false, other, "");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isPlayerNearby = null;
        if (InteractionButtonOutside)
            InteractionButtonOutside.ShowButtonUI(false, other, "");
        if (InteractionButtonInside)
            InteractionButtonInside.ShowButtonUI(false, other, "");
    }
}
