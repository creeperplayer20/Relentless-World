using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Components")]
    private Rigidbody rb;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform groundCheck;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private float jumpCooldownDuration = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Look")]
    [Tooltip("Základní citlivost kamery. Pro Gamepad uprav citlivost pomocí 'Processors -> Scale Vector 2' přímo v Input Action Assetu.")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;
    private bool isGrounded;
    private bool jumpRequested;
    private float nextJumpTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraPivot == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }

        playerMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
        moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);

        if (groundCheck == null) Debug.LogError($"{nameof(PlayerMovement)}: GroundCheck is not assigned.", this);
        if (inputActions == null) Debug.LogError($"{nameof(PlayerMovement)}: InputActions is not assigned.", this);
    }

    private void OnEnable()
    {
        playerMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        playerMap.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        isGrounded = CheckGrounded();

        TryJump();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();

        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void Move()
    {
        Vector3 localMove = new(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = transform.TransformDirection(localMove);
        rb.MovePosition(rb.position + Time.fixedDeltaTime * walkSpeed * worldMove);
    }

    private void Look()
    {
        if (cameraPivot == null) return;

        float yawDegrees = lookInput.x * lookSensitivity;
        float pitchDegrees = lookInput.y * lookSensitivity;

        transform.Rotate(0f, yawDegrees, 0f, Space.World);

        pitch -= pitchDegrees;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void TryJump()
    {
        if (Time.time < nextJumpTime) return;
        if (!isGrounded) return;
        if (!jumpAction.WasPressedThisFrame()) return;

        jumpRequested = true;
        nextJumpTime = Time.time + jumpCooldownDuration;
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        return Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }
}
