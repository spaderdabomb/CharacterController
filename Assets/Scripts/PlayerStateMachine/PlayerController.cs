
using System;
using System.Linq;
using Medicine;
using SpaceGame.StateMachine;
using SpaceGame.StateMachine.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using GinjaGaming.FirstPersonCharacterController;
// ReSharper disable Unity.NoNullPropagation

namespace SpaceGame
{
	[RequireComponent(typeof(StateController), typeof(CharacterController))]
	[DefaultExecutionOrder(1)]
	[Register.Single]
	public class PlayerController : MonoBehaviour, PlayerControls.IPlayerMapActions
	{
		#region Fields
		[Header("Fields")]
		[SerializeField] private StateController m_StateController = null!;
		[field: SerializeField] public CharacterController m_CharacterController { get; private set; } = null!;
        [field: SerializeField] public Camera m_Camera { get; private set; } = null!;
        [field: SerializeField] public Animator Animator { get; private set; } = null!;

        public PlayerControls MainControls;

        #endregion

        #region Properties

        [Header("Camera")]
        public float LookSenseH = 0.1f;
        public float LookSenseV = 0.1f;
        public float LookUpLimit = 70f;
        public float LookDownLimit = -70f;

        [Header("Locomotion")]
		public float IsMovingThreshold = 0.01f;
        public float Drag = 0.25f;
        public float Gravity = 25f;

        [Header("Settings")]
        [SerializeField] private bool HoldToSprint = true;

        [Header("Environment Details")]
        [SerializeField] private LayerMask GroundLayers;

        #endregion

        #region Member Variables

        public Vector2 m_MovementInput = Vector2.zero;
        private Vector2 m_LookInput = Vector2.zero;
        private Vector2 m_CameraRotation = Vector2.zero;
        private Vector2 m_PlayerRotation = Vector2.zero;

        private bool m_IsSprinting = false;
        private bool m_JumpPressed = false;
        private bool m_SprintPressed = false;
        private bool m_CanSprint = false;
        private bool m_CanJump = false;

        #endregion

        #region Events

        #endregion

        #region Functions

        private void OnEnable()
		{
            MainControls = new PlayerControls();
            MainControls.PlayerMap.Enable();
			MainControls.PlayerMap.SetCallbacks(this);
        }

        private void OnDisable()
		{
            MainControls.PlayerMap.Disable();
            MainControls.PlayerMap.RemoveCallbacks(this);
        }

        private void Start()
        {
            m_StateController.SetStateActive<PlayerIdleState>();
        }

        private void Update()
        {
			CheckLateralMovementState();
        }

        private void LateUpdate()
        {
            m_CameraRotation.x += LookSenseH * m_LookInput.x;
            m_CameraRotation.y = Mathf.Clamp(m_CameraRotation.y - LookSenseV * m_LookInput.y, LookDownLimit, LookUpLimit);

            m_PlayerRotation.x += transform.eulerAngles.x + LookSenseH * m_LookInput.x;

            transform.rotation = Quaternion.Euler(0f, m_PlayerRotation.x, 0f);
            m_Camera.transform.rotation = Quaternion.Euler(m_CameraRotation.y, m_CameraRotation.x, 0f);
        }

        private void CheckLateralMovementState()
		{
			if (m_MovementInput == Vector2.zero && !IsMovingLaterally())
            {
                return;
            }


            /*            if (m_SprintPressed)
                        {
                            m_StateController.SetStateActive<PlayerSprintState>();
                        }
                        else
                        {
                            m_StateController.SetStateActive<PlayerRunState>();
                        }*/
        }

        #endregion

        #region Input

        public void OnLook(InputAction.CallbackContext context)
		{
            m_LookInput = context.ReadValue<Vector2>();
        }

        public void OnMovement(InputAction.CallbackContext context)
		{
			m_MovementInput = context.ReadValue<Vector2>();
		}

		public void OnToggleSprint(InputAction.CallbackContext context)
		{
            if (context.performed)
            {
                m_SprintPressed = HoldToSprint || !m_IsSprinting;
            }
            else if (context.canceled)
            {
                m_SprintPressed = !HoldToSprint && m_SprintPressed;
            }
        }

		public void OnJump(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			m_StateController.SetStateActive<PlayerJumpState>();
		}

		#endregion

		#region Helper Functions

		public StateController GetStateController()
		{
			return m_StateController;
		}

		public bool IsMovingLaterally()
		{
            Vector3 lateralVelocity = new Vector3(m_CharacterController.velocity.x, 0f, m_CharacterController.velocity.z);

			return lateralVelocity.magnitude > IsMovingThreshold;
        }

		public bool IsSprinting()
		{
			return m_SprintPressed;
		}

		public bool IsGrounded()
		{
			return m_CharacterController.isGrounded;

        }

		#endregion

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (m_StateController == null)
			{
				m_StateController = GetComponent<StateController>();
			}

			if (m_CharacterController == null)
			{
                m_CharacterController = GetComponent<CharacterController>();
			}

			if (m_Camera == null)
			{
				m_Camera = GetComponentInChildren<Camera>();
			}
		}

        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
