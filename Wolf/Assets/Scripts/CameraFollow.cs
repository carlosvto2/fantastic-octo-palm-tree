using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to player
    public Vector3 offset; // Movement

    void LateUpdate()
    {
        transform.position = player.position + offset; // Update camera position
    }
}
