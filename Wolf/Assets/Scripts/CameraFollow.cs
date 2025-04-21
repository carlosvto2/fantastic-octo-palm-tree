using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to playerpublic
    float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 5f;

    private float currentX = 0f;
    private float currentY = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Hide cursor
    }

    void LateUpdate()
    {
        if (!player) return;

        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, 5, 75);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 dir = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = player.position + Vector3.up * height + dir;

        transform.position = desiredPosition;
        transform.LookAt(player.position + Vector3.up * 1.5f); // Mira al centro del cuerpo
    }
}
