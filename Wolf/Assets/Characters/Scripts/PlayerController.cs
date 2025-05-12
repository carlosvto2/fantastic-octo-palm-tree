using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 10f;            // Movement speed
    protected Transform cam;                  // Reference to the main camera's transform
    protected Animator animator;             // Reference to the Animator component

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>(); // Assumes Animator is on a child model
    }

    protected virtual void Update(){}

    protected virtual void FixedUpdate(){}

    public void CreateCamera(GameObject cameraPrefab){
        // Create camera and assign player
        GameObject createdCamera = Instantiate(cameraPrefab);
        CameraFollow camScript = createdCamera.GetComponent<CameraFollow>();
        camScript.player = transform;

        cam = createdCamera.transform;
    }
}
