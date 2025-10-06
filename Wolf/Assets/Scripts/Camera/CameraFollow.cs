using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public float distance = 10f;      // distancia al jugador
    public float height = 5f;         // altura base
    public float rotationSpeed = 5f;  // sensibilidad mouse
    public float minHeightAngle = 20f;
    public float maxHeightAngle = 60f;

    private float currentX = 45f; // ángulo horizontal inicial
    private float currentY = 30f; // ángulo vertical inicial

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!player) return;

        // --- Rotación con mouse ---
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, minHeightAngle, maxHeightAngle);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Posición deseada
        Vector3 desiredPosition = player.position + rotation * new Vector3(0, 0, -distance) + Vector3.up * height;

        transform.position = desiredPosition;

        // Siempre mirar al jugador
        transform.LookAt(player.position + Vector3.up * height * 0.5f); // mirar un poco por encima del centro
    }

    public void SetTarget(Transform PlayerLocation)
    {
        player = PlayerLocation;
    }
}
