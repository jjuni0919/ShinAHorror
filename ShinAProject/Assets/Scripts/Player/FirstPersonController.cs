using System.Collections.Generic;
using ShinA.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShinA.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const float GroundedVerticalVelocity = -2f;

        [Header("참조")]
        [SerializeField] private Camera playerCamera;

        [Header("이동")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float runSpeed = 7f;
        [SerializeField, Min(0f)] private float crouchSpeed = 2.2f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -24f;

        [Header("앉기")]
        [SerializeField, Min(0.5f)] private float standingHeight = 1.8f;
        [SerializeField, Min(0.5f)] private float crouchingHeight = 1.1f;
        [SerializeField, Min(0f)] private float standingCameraHeight = 1.65f;
        [SerializeField, Min(0f)] private float crouchingCameraHeight = 1f;
        [SerializeField, Min(0.1f)] private float crouchTransitionSpeed = 10f;

        [Header("시점")]
        [SerializeField, Range(1f, 89f)] private float maxLookAngle = 85f;

        [Header("스태미나")]
        [SerializeField, Min(0.1f)] private float maxStamina = 5f;
        [SerializeField, Min(0f)] private float staminaDrainPerSecond = 1f;
        [SerializeField, Min(0f)] private float staminaRecoveryPerSecond = 0.8f;
        [SerializeField, Min(0f)] private float jumpStaminaCost = 1f;
        [SerializeField, Range(0f, 1f)] private float exhaustionRecoveryThreshold = 0.4f;

        [Header("낮은 체력")]
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.2f;
        [SerializeField, Range(0f, 1f)] private float lowHealthSpeedMultiplier = 0.5f;

        private CharacterController characterController;
        private PlayerHealth playerHealth;
        private readonly HashSet<object> gameplayInputBlockers = new();
        private float verticalVelocity;
        private float cameraPitch;
        private float currentStamina;
        private bool staminaDepleted;
        private bool gameplayInputEnabled = true;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 0f;
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public float MovementAmount { get; private set; }
        public bool CanAct => GameplayInputEnabled;
        public bool GameplayInputEnabled => gameplayInputEnabled && gameplayInputBlockers.Count == 0;

        public void Initialize(Camera cameraToUse)
        {
            playerCamera = cameraToUse;
        }

        public void SetGameplayInputEnabled(bool enabled)
        {
            gameplayInputEnabled = enabled;
            ApplyGameplayInputState();
        }

        internal void SetGameplayInputBlocked(object owner, bool blocked)
        {
            if (owner == null)
            {
                return;
            }

            bool changed = blocked
                ? gameplayInputBlockers.Add(owner)
                : gameplayInputBlockers.Remove(owner);
            if (changed)
            {
                ApplyGameplayInputState();
            }
        }

        private void ApplyGameplayInputState()
        {
            IsRunning = false;
            MovementAmount = 0f;
            if (isActiveAndEnabled)
            {
                SetCursorLocked(GameplayInputEnabled);
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerHealth = GetComponent<PlayerHealth>();
            currentStamina = maxStamina;

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void OnEnable()
        {
            SetCursorLocked(GameplayInputEnabled);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (!GameplayInputEnabled)
            {
                ApplyGravityOnly();
                return;
            }

            HandleCursor();
            HandleLook();
            HandleMovement();
        }

        private void HandleCursor()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }

        private void HandleLook()
        {
            if (playerCamera == null || Mouse.current == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 lookDelta = Mouse.current.delta.ReadValue() * GameSettings.LookSensitivity;
            transform.Rotate(Vector3.up, lookDelta.x, Space.World);

            cameraPitch = Mathf.Clamp(cameraPitch - lookDelta.y, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            bool wasExhausted = staminaDepleted;
            if (staminaDepleted)
            {
                UpdateStamina(false);
                if (currentStamina >= maxStamina * exhaustionRecoveryThreshold)
                {
                    staminaDepleted = false;
                }
            }

            UpdateCrouch();

            Vector2 input = ReadMovementInput();
            MovementAmount = Mathf.Clamp01(input.magnitude);
            Vector3 planarDirection = transform.right * input.x + transform.forward * input.y;
            planarDirection = Vector3.ClampMagnitude(planarDirection, 1f);

            bool hasMovementInput = planarDirection.sqrMagnitude > 0.001f;
            bool runHeld = PlayerInputBindings.IsPressed(PlayerAction.Run);

            bool lowHealth = playerHealth != null && playerHealth.HealthNormalized <= lowHealthThreshold;
            IsRunning = hasMovementInput && runHeld && !IsCrouching && !lowHealth &&
                        !staminaDepleted && currentStamina > 0f;
            if (!wasExhausted)
            {
                UpdateStamina(IsRunning);
            }

            float speed = IsCrouching ? crouchSpeed : IsRunning ? runSpeed : walkSpeed;
            if (lowHealth)
            {
                speed *= lowHealthSpeedMultiplier;
            }
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVerticalVelocity;
            }

            if (characterController.isGrounded &&
                PlayerInputBindings.WasPressedThisFrame(PlayerAction.Jump) && !IsCrouching && !staminaDepleted)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                ConsumeStamina(jumpStaminaCost);
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = planarDirection * speed;
            velocity.y = verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void ApplyGravityOnly()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVerticalVelocity;
            }

            verticalVelocity += gravity * Time.deltaTime;
            characterController.Move(Vector3.up * (verticalVelocity * Time.deltaTime));
        }

        private void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - Mathf.Max(0f, amount));
            if (currentStamina <= 0f)
            {
                staminaDepleted = true;
                IsRunning = false;
            }
        }

        private void UpdateCrouch()
        {
            bool crouchHeld = PlayerInputBindings.IsPressed(PlayerAction.Crouch);

            if (crouchHeld)
            {
                IsCrouching = true;
            }
            else if (IsCrouching && CanStandUp())
            {
                IsCrouching = false;
            }

            float targetHeight = IsCrouching ? crouchingHeight : standingHeight;
            characterController.height = Mathf.MoveTowards(
                characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            characterController.center = Vector3.up * (characterController.height * 0.5f);

            if (playerCamera != null)
            {
                float targetCameraHeight = IsCrouching ? crouchingCameraHeight : standingCameraHeight;
                Vector3 cameraPosition = playerCamera.transform.localPosition;
                cameraPosition.y = Mathf.MoveTowards(
                    cameraPosition.y, targetCameraHeight, crouchTransitionSpeed * Time.deltaTime);
                playerCamera.transform.localPosition = cameraPosition;
            }
        }

        private bool CanStandUp()
        {
            float radius = characterController.radius * 0.95f;
            Vector3 bottom = transform.position + Vector3.up * radius;
            Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
            Collider[] overlaps = Physics.OverlapCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore);

            foreach (Collider overlap in overlaps)
            {
                if (overlap != characterController && !overlap.transform.IsChildOf(transform))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateStamina(bool running)
        {
            if (running)
            {
                ConsumeStamina(staminaDrainPerSecond * Time.deltaTime);
            }
            else
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRecoveryPerSecond * Time.deltaTime);
            }
        }

        private static Vector2 ReadMovementInput()
        {
            float horizontal = (PlayerInputBindings.IsPressed(PlayerAction.MoveRight) ? 1f : 0f) -
                               (PlayerInputBindings.IsPressed(PlayerAction.MoveLeft) ? 1f : 0f);
            float vertical = (PlayerInputBindings.IsPressed(PlayerAction.MoveForward) ? 1f : 0f) -
                             (PlayerInputBindings.IsPressed(PlayerAction.MoveBackward) ? 1f : 0f);
            return new Vector2(horizontal, vertical);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
