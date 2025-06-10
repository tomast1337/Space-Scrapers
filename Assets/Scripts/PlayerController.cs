using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedFactor = 3.58475257f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 movementInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool moveY = Input.GetButtonDown("Jump");
        bool sprint = Input.GetButton("Fire3");

        movementInput = new Vector3(moveX, 0f, moveZ).normalized * (sprint ? sprintSpeedFactor : 1f);

        // Ground check using raycast
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);


        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Apply movement
        Vector3 move = transform.forward * movementInput.z + transform.right * movementInput.x;
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.linearVelocity.y; // preserve y velocity
        rb.linearVelocity = velocity;
    }


    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
    }
}
