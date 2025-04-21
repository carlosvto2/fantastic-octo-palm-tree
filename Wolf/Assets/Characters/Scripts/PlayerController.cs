using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;            // Movement speed
    private CharacterController controller; // Reference to the CharacterController
    private Transform cam;                  // Reference to the main camera's transform
    private Animator animator;              // Reference to the Animator component

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        animator = GetComponentInChildren<Animator>(); // Assumes Animator is on a child model
    }

    void Update()
    {
        // Read input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Calculate camera-relative direction
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Flatten the direction vectors so the player doesn't move vertically
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Combine input with camera direction
        Vector3 move = camForward * v + camRight * h;

        // Move the character
        if (move.magnitude > 0.1f)
        {
            controller.Move(move.normalized * moveSpeed * Time.deltaTime);

            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
        }

        // Update animation
        if (animator != null)
        {
            float speed = move.magnitude;
            animator.SetFloat("Speed", speed > 0.1f ? 1f : 0f);
        }
    }
}
