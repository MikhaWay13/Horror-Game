using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Range(1f, 89f)] private float maxLookAngle = 85f;

    [Header("Head Bob")]
    [SerializeField] private float bobSpeed = 14f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Footsteps")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private float stepInterval = 0.5f;

    private float stepTimer;

    private CharacterController characterController;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float cameraPitch;

    private float defaultCameraY;
    private float bobTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        controls = new PlayerControls();

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        if (cameraHolder == null && cameraTransform != null)
        {
            cameraHolder = cameraTransform.parent;
        }

        if (cameraHolder != null)
        {
            defaultCameraY = cameraHolder.localPosition.y;
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        LockCursor();
    }

    private void OnDisable()
    {
        controls.Player.Disable();

        UnlockCursor();
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void Update()
    {
        ReadInput();

        HandleLook();
        HandleMovement();
        HandleHeadBob();
        HandleFootsteps();
    }

    private void ReadInput()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        lookInput = controls.Player.Look.ReadValue<Vector2>();
    }

    private void HandleLook()
    {
        Vector2 lookDelta = lookInput * mouseSensitivity;

        transform.Rotate(Vector3.up * lookDelta.x);

        cameraPitch -= lookDelta.y;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (isGrounded &&
            controls.Player.Jump.WasPressedThisFrame())
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection * moveSpeed;

        velocity.y = verticalVelocity;

        characterController.Move(
            velocity * Time.deltaTime);
    }

   private void HandleFootsteps()
{
    if (footstepEvent.IsNull)
        return;

    if (!characterController.isGrounded)
        return;

    Vector3 horizontalVelocity = characterController.velocity;
    horizontalVelocity.y = 0f;

    bool isMoving = horizontalVelocity.magnitude > 0.1f;

    if (!isMoving)
    {
        stepTimer = stepInterval;
        return;
    }

    stepTimer -= Time.deltaTime;

    if (stepTimer <= 0f)
    {
        RuntimeManager.PlayOneShot(
            footstepEvent,
            transform.position);

        stepTimer = stepInterval;
    }
}
    private void HandleHeadBob()
    {
        if (cameraHolder == null)
            return;

        if (!characterController.isGrounded)
            return;

        Vector2 horizontalVelocity = new Vector2(
            characterController.velocity.x,
            characterController.velocity.z);

        if (horizontalVelocity.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            Vector3 holderPosition =
                cameraHolder.localPosition;

            holderPosition.y =
                defaultCameraY +
                Mathf.Sin(bobTimer) * bobAmount;

            cameraHolder.localPosition =
                holderPosition;
        }
        else
        {
            bobTimer = 0f;

            Vector3 holderPosition =
                cameraHolder.localPosition;

            holderPosition.y = Mathf.Lerp(
                holderPosition.y,
                defaultCameraY,
                Time.deltaTime * bobSpeed);

            cameraHolder.localPosition =
                holderPosition;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }
}