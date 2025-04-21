using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 700f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Calculate the input velocity for the animations
        float inputMagnitude = moveDirection.magnitude;
        animator.SetFloat("Speed", inputMagnitude, 0f, Time.deltaTime); // Activate the animation

        // Check if there is movement input
        if (moveDirection.magnitude >= 0.1f)
        {
            // Calculate the rotation angle towards the movment direction
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            // Rotate to the angle target
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move the character forward
            Vector3 move = transform.forward * moveSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
        }
    }
}
