using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
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
    private CharacterController characterController;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerStats stats;

    [Header("Move")]
    [Tooltip("Walking speed in units per second!")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private float jumpCooldownDuration = 0.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Costs")]
    [SerializeField] private float jumpStaminaCost = 20f;
    [SerializeField] private float sprintStaminaCostPerSecond = 15f;

    [Header("Look")]
    [Tooltip("Sensitivity multiplier for looking around. Higher values make the camera more responsive to input.")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;
    private bool isGrounded;
    private float nextJumpTime;
    private bool isSprinting;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

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

        isGrounded = characterController.isGrounded;

        bool wantsSprint = sprintAction != null && sprintAction.IsPressed();
        isSprinting = wantsSprint && moveInput.sqrMagnitude > 0.0001f && stats != null && stats.Stamina > 0.01f;

        if (stats != null)
        {
            if (isSprinting)
            {
                if (!stats.TrySpendStamina(sprintStaminaCostPerSecond * Time.deltaTime))
                    isSprinting = false;
            }
            else
            {
                stats.RegenStamina(20f * Time.deltaTime);
            }
        }

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = Mathf.Max(verticalVelocity, -50f);

        TryJump();
        ApplyGravity();
        Move();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Move()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 localMove = new(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = transform.TransformDirection(localMove);

        Vector3 velocity = worldMove * speed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
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

        verticalVelocity = jumpImpulse;

        nextJumpTime = Time.time + jumpCooldownDuration;
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }
}
