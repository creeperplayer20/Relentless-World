using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Components")]
    private Rigidbody rb;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private PlayerStats stats;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private float jumpCooldownDuration = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Costs")]
    [SerializeField] private float jumpStaminaCost = 20f;
    [SerializeField] private float sprintStaminaCostPerSecond = 15f;

    [Header("Look")]
    [Tooltip("Základní citlivost kamery. Pro Gamepad uprav citlivost pomocí 'Processors -> Scale Vector 2' přímo v Input Action Assetu.")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;
    private bool isGrounded;
    private bool jumpRequested;
    private float nextJumpTime;
    private bool isSprinting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (stats == null) stats = GetComponent<PlayerStats>();

        if (cameraPivot == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }

        playerMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
        moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);

        sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: false);

        if (groundCheck == null) Debug.LogError($"{nameof(PlayerMovement)}: GroundCheck is not assigned.", this);
        if (inputActions == null) Debug.LogError($"{nameof(PlayerMovement)}: InputActions is not assigned.", this);
        if (stats == null) Debug.LogError($"{nameof(PlayerMovement)}: PlayerStats is missing on this GameObject.", this);
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

        bool wantsSprint = sprintAction != null && sprintAction.IsPressed();
        isSprinting = wantsSprint && moveInput.sqrMagnitude > 0.0001f && stats != null && stats.Stamina > 0.01f;

        TryJump();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (isSprinting && stats != null)
        {
            float cost = sprintStaminaCostPerSecond * dt;
            if (!stats.TrySpendStamina(cost)) isSprinting = false;
        }
        else if (stats != null)
        {
            stats.RegenStamina(dt);
        }

        Move();

        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void Move()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 localMove = new(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = transform.TransformDirection(localMove);
        rb.MovePosition(rb.position + Time.fixedDeltaTime * speed * worldMove);
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

        if (stats != null && !stats.TrySpendStamina(jumpStaminaCost)) return;

        jumpRequested = true;
        nextJumpTime = Time.time + jumpCooldownDuration;
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        return Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }
}
