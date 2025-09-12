using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Jugador a seguir

    [Header("Camera Settings")]
    public float distance = 5f;       // Distancia detrás del jugador
    public float height = 2f;         // Altura relativa al jugador
    public float rotationSpeed = 5f;  // Velocidad de rotación con el mouse
    public float minHeightAngle = -75f;
    public float maxHeightAngle = 75f;

    [Header("Collision Settings")]
    public float cameraRadius = 0.3f;      // Radio para colisiones
    public LayerMask collisionLayers;      // Capas que bloquean cámara

    private float currentX = 0f;
    private float currentY = 15f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Oculta cursor
    }

    private void LateUpdate()
    {
        if (!player) return;

        // --- Rotación con el ratón ---
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, minHeightAngle, maxHeightAngle);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Posición base de la cámara (antes de colisiones)
        Vector3 origin = player.position + Vector3.up * height;
        Vector3 desiredDir = rotation * Vector3.back;
        Vector3 desiredPosition = origin + desiredDir * distance;

        // --- Colisión con paredes/techos ---
        RaycastHit hit;
        if (Physics.SphereCast(origin, cameraRadius, desiredDir, out hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // Coloca la cámara al punto de impacto, pero un poco antes
            desiredPosition = hit.point + hit.normal * cameraRadius;
        }

        // --- Colocar cámara ---
        transform.position = desiredPosition;
        transform.rotation = rotation;

        // Siempre mira al jugador
        transform.LookAt(origin);
    }

    public void SetTarget(Transform PlayerLocation)
    {
        player = PlayerLocation;
    }
}
