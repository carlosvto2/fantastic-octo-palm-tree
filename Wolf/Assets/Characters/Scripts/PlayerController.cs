using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 10f;            // Movement speed
    private CharacterController controller; // Reference to the CharacterController
    protected Transform cam;                  // Reference to the main camera's transform
    protected Animator animator;             // Reference to the Animator component

    protected virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); // Assumes Animator is on a child model
    }

    protected virtual void Update()
    {
        // Read input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate camera-relative direction
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Flatten the direction vectors so the player doesn't move vertically
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Combine input with camera direction
        Vector3 move = camForward * verticalInput + camRight * horizontalInput;

        Debug.Log("verticalInput: " + verticalInput);
        Debug.Log("horizontalInput: " + horizontalInput);
        Debug.Log("Move: " + move);

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

    protected virtual void FixedUpdate(){}

    public void CreateCamera(GameObject cameraPrefab){
        // Create camera and assign player
        GameObject createdCamera = Instantiate(cameraPrefab);
        CameraFollow camScript = createdCamera.GetComponent<CameraFollow>();
        camScript.player = transform;

        cam = createdCamera.transform;
    }
}
