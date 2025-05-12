using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerWolf : PlayerController
{
    public float rotationSpeed = 10f;   // Speed of rotation to face movement
    private Rigidbody rb;     // Animator reference

    private Vector3 movementInput;      // Input stored

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        if(cam == null)
            return;

        // Read input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate the forward and right direction based on the camera's orientation
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0; // Remove any vertical component
        camRight.y = 0;
        camForward.Normalize(); // Normalize to ensure we don't get uneven speeds
        camRight.Normalize();

        // Combine forward and right vectors with player input to get movement direction
        movementInput = camForward * vertical + camRight * horizontal;
        movementInput.Normalize(); // Normalize the movement direction for consistent speed

        // Update animator parameters
        if (animator != null)
        {
            // Calculate speed based on input magnitude (length of the movement vector)
            float speed = new Vector2(horizontal, vertical).magnitude;
            animator.SetFloat("Speed", speed); // Assuming you have a "Speed" float parameter
        }
    }

    protected override void FixedUpdate()
    {
        // Move the Rigidbody in the direction of the movement input
        // Use the 'y' velocity from Rigidbody to keep gravity applied naturally
        Vector3 moveVelocity = movementInput * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        // Rotate to face movement direction
        if (movementInput != Vector3.zero)
        {
            // Calculate the desired rotation based on movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            // Smoothly rotate the player towards the target rotation
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}