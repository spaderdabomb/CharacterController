using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

namespace GinjaGaming.FirstPersonCharacterController
{
    [DefaultExecutionOrder(1)]
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public class PlayerController : SerializedMonoBehaviour, PlayerControls.IPlayerMapActions
    {
        #region Properties
        public CharacterController CharacterController { get; private set; }
        public Camera PlayerCamera { get; private set; }
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        public PlayerControls PlayerControls { get; private set; }
        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;
        #endregion

        #region Editor
        [Header("Base Movement")]
        public float walkAcceleration = 0.15f;
        public float walkSpeed = 2f;
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float sprintAcceleration = 0.5f;
        public float sprintSpeed = 7f;
        public float drag = 0.1f;
        public float gravity = 25f;
        public float jumpSpeed = 1.0f;
        public float movingThreshold = 0.01f;
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.25f;

        [Header("Camera")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        [Header("Settings")]
        [SerializeField] private float slopeForce = 100f;
        [SerializeField] private float groundDetectionHeight = 0.1f;
        [SerializeField] private bool _holdToSprint = true;
        [SerializeField, ReadOnly] private bool _walkToggledOn = false;
        [SerializeField, ReadOnly] private bool _sprintToggledOn = false;

        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;
        #endregion

        #region Members
        private PlayerState _playerState;
        private CapsuleCollider _groundCollider;

        private Vector2 _lookInput = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        private Vector2 _cameraRotation = Vector2.zero;

        public Vector2 deltaRotation = Vector2.zero;

        private bool _jumpPressed = false;
        private bool _jumpedLastFrame = false;
        private float _rotatingToTargetTimer = 0f;
        private float _groundedTimer = 0;
        private float _verticalVelocity = 0f;
        #endregion

        #region Startup 
        private void Awake()
        {
            PlayerCamera = GetComponentInChildren<Camera>();
            CharacterController = GetComponent<CharacterController>();

            _playerState = GetComponent<PlayerState>();
            _groundCollider = GetComponent<CapsuleCollider>();
        }

        private void OnEnable()
        {
            PlayerControls = new PlayerControls();
            PlayerControls.Enable();
            PlayerControls.PlayerMap.Enable();
            PlayerControls.PlayerMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            PlayerControls.PlayerMap.Disable();
            PlayerControls.PlayerMap.RemoveCallbacks(this);
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _playerState.InGroundedState();
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;

            float groundedGravity = (isGrounded && !_jumpPressed && !isFalling) ? slopeForce : 0f;

            // Setup grounded timer to prevent jumping in consecutive frames
            if (isGrounded)
                _groundedTimer = 0.2f;

            if (_groundedTimer > 0)
                _groundedTimer -= Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0)
                _verticalVelocity = 0f;

            _verticalVelocity -= (gravity + groundedGravity) * Time.deltaTime;

            if (_jumpPressed && _groundedTimer > 0)
            {
                _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);

                _groundedTimer = 0;
                _jumpPressed = false;
                _jumpedLastFrame = true;
            }
        }

        private void HandleVerticalMovement2()
        {
            bool isGrounded = _playerState.InGroundedState();

            // Setup grounded timer to prevent jumping in consecutive frames
            if (isGrounded)
                _groundedTimer = 0.2f;

            if (_groundedTimer > 0)
                _groundedTimer -= Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0)
                _verticalVelocity = 0f;

            _verticalVelocity -= gravity * Time.deltaTime;

            if (_jumpPressed && _groundedTimer > 0)
            {
                _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);

                _groundedTimer = 0;
                _jumpPressed = false;
                _jumpedLastFrame = true;
            }
        }

        private void HandleLateralMovement()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

            // State dependent acceleration and speed
            float currentLateralAcceleration = isWalking ? walkAcceleration : 
                                               isSprinting ? sprintAcceleration : runAcceleration;

            float clampLateralVelocityMagnitude = isWalking ? walkSpeed : 
                                                  isSprinting ? sprintSpeed : runSpeed;

            // Get lateral movement from input
            Vector3 lateralVelocity = new Vector3(CharacterController.velocity.x, 0f, CharacterController.velocity.z);
            Vector3 cameraForwardProjectedXZ = new Vector3(PlayerCamera.transform.forward.x, 0f, PlayerCamera.transform.forward.z).normalized;
            Vector3 cameraRightProjectedXZ = new Vector3(PlayerCamera.transform.right.x, 0f, PlayerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightProjectedXZ * MovementInput.x + cameraForwardProjectedXZ * MovementInput.y;
            Vector3 movementDelta = movementDirection * currentLateralAcceleration;

            // Clamp velocity to 0 if barely moving and not using input
            if (lateralVelocity.magnitude < movingThreshold && MovementInput == Vector2.zero)
            {
                lateralVelocity = Vector3.zero;
            }

            lateralVelocity += movementDelta;

            Vector3 playerVelocityNew = Vector3.zero;
            playerVelocityNew += lateralVelocity;
            playerVelocityNew -= playerVelocityNew.normalized * drag;
            playerVelocityNew = (playerVelocityNew.magnitude > drag) ? playerVelocityNew : Vector3.zero;
            playerVelocityNew = Vector3.ClampMagnitude(playerVelocityNew, clampLateralVelocityMagnitude);

            playerVelocityNew.y = _verticalVelocity;

            CharacterController.Move(playerVelocityNew * Time.deltaTime);
        }

        private Vector3 GetCharacterControllerNormal()
        {
            // Get normal
            Vector3 normal = Vector3.zero;
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, Vector3.down, 0.1f, _groundLayers);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                normal = hit.normal;
            }

            return normal;
        }

        private void HandleLateralMovement2()
        {
            bool isGrounded = _playerState.InGroundedState();
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

            // State dependent acceleration and speed
            float currentLateralAcceleration = isWalking ? walkAcceleration :
                                               isSprinting ? sprintAcceleration : runAcceleration;

            float clampLateralVelocityMagnitude = isWalking ? walkSpeed :
                                                  isSprinting ? sprintSpeed : runSpeed;

            // Get lateral movement from input
            Vector3 lateralVelocity = CharacterController.velocity;
            Vector3 cameraForwardProjectedXZ = new Vector3(PlayerCamera.transform.forward.x, 0f, PlayerCamera.transform.forward.z).normalized;
            Vector3 cameraRightProjectedXZ = new Vector3(PlayerCamera.transform.right.x, 0f, PlayerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightProjectedXZ * MovementInput.x + cameraForwardProjectedXZ * MovementInput.y;
            Vector3 movementDelta = movementDirection * currentLateralAcceleration;

            // Clamp velocity to 0 if barely moving and not using input
            if (lateralVelocity.magnitude < movingThreshold && MovementInput == Vector2.zero)
            {
                lateralVelocity = Vector3.zero;
            }
            lateralVelocity += movementDelta;

            Vector3 playerVelocityNew = lateralVelocity;
            playerVelocityNew -= playerVelocityNew.normalized * drag;
            playerVelocityNew = (playerVelocityNew.magnitude > drag) ? playerVelocityNew : Vector3.zero;
            playerVelocityNew = Vector3.ClampMagnitude(playerVelocityNew, clampLateralVelocityMagnitude);

            if (isGrounded)
            {
                Vector3 normal = GetCharacterControllerNormal();
                playerVelocityNew = Vector3.ProjectOnPlane(playerVelocityNew, normal);
                playerVelocityNew.y += _verticalVelocity;
            }
            else
            {
                playerVelocityNew.y = _verticalVelocity;
            }

            CharacterController.Move(playerVelocityNew * Time.deltaTime);
        }

        private void UpdateMovementState()
        {
            bool canRun = CanRun();
            bool isMovementInput = MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();                      //order
            bool isSprinting = _sprintToggledOn && isMovingLaterally;          //matters
            bool isWalking = (isMovingLaterally && !canRun) || _walkToggledOn; //order

            // Control Move State
            bool isGrounded = IsGrounded();                                    // order matters
            PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                               isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            _playerState.SetPlayerMovementState(lateralState);

            // Control Jump State
            if ((!isGrounded || _jumpedLastFrame) && CharacterController.velocity.y >= 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                _jumpedLastFrame = false;
                _jumpPressed = false;
            }
            else if ((!isGrounded || _jumpedLastFrame) && CharacterController.velocity.y < 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                _jumpedLastFrame = false;
                _jumpPressed = false;
            }
        }

        #endregion

        #region LateUpdate Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _cameraRotation.x += lookSenseH * _lookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _lookInput.y, -lookLimitV, lookLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _lookInput.x;
            deltaRotation = _lookInput * lookSenseH;

            // If we are not idling, have a rotation mismatch in tolerance, and rotate to target timer is activated, rotate
            if (_playerState.CurrentPlayerMovementState != PlayerMovementState.Idling || Mathf.Abs(RotationMismatch) > 90f || _rotatingToTargetTimer > 0)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, _playerTargetRotation.x, 0f), playerModelRotationSpeed * Time.deltaTime);

                if (Mathf.Abs(RotationMismatch) > 90f)
                {
                    _rotatingToTargetTimer = rotateToTargetTime;
                }
                _rotatingToTargetTimer -= Time.deltaTime;
            }

            IsRotatingToTarget = _rotatingToTargetTimer > 0;

            PlayerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            // Get angle between camera and player
            Vector3 cameraForwardProjectedXZ = new Vector3(PlayerCamera.transform.forward.x, 0f, PlayerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, cameraForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, cameraForwardProjectedXZ);
        }

        #endregion

        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(CharacterController.velocity.x, 0f, CharacterController.velocity.z);

            return (lateralVelocity.magnitude > movingThreshold);
        }

        private bool IsGrounded()
        {
            bool grounded;

            if (_playerState.InGroundedState())
            {
                Collider[] colliders = Physics.OverlapCapsule(
                    _groundCollider.transform.TransformPoint(_groundCollider.center - Vector3.up * _groundCollider.height * 0.5f),
                    _groundCollider.transform.TransformPoint(_groundCollider.center + Vector3.up * _groundCollider.height * 0.5f),
                    _groundCollider.radius,
                    _groundLayers
                );

                grounded = colliders.Length > 0;
            }
            else
            {
                grounded = CharacterController.isGrounded;
            }

            return grounded;
        }

        private bool CanRun()
        {
            return MovementInput.y >= Mathf.Abs(MovementInput.x);
        }

        #endregion

        #region Input
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnToggleSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _sprintToggledOn = _holdToSprint || !_sprintToggledOn;
                _walkToggledOn = false;
            }
            else if (context.canceled)
            {
                _sprintToggledOn = !_holdToSprint && _sprintToggledOn;
                _walkToggledOn = false;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _jumpPressed = true;
        }

        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _walkToggledOn = !_walkToggledOn;
        }
        #endregion

        #region Validation
        private void OnValidate()
        {

        }

        #endregion
    }
}
