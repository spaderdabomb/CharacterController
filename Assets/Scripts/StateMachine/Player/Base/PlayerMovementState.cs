
using Unity.VisualScripting;
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public abstract class PlayerMovementState : PlayerState
	{
		protected CharacterController m_CharacterController = null!;
        protected Camera m_PlayerCamera = null!;
        protected Animator m_Animator = null!;

        protected override void OnInitializeState()
		{
			base.OnInitializeState();
            m_CharacterController = m_PlayerController.m_CharacterController;
			m_PlayerCamera = m_PlayerController.m_Camera;
			m_Animator = m_PlayerController.Animator;

        }

		protected override void OnStartState()
		{
			base.OnStartState();
		}

		protected override void OnStopState(bool bTransition = false)
		{
			base.OnStopState(bTransition);
		}

		protected void MovePlayer(Vector2 inputDirection, float moveSpeed, float acceleration)
		{
			// Get lateral movement from input
			Vector3 movementDirection = m_CharacterController.transform.right * inputDirection.x + m_CharacterController.transform.forward * inputDirection.y;
            Vector3 movementDelta = movementDirection * acceleration;
            Vector3 lateralVelocity = new Vector3(m_CharacterController.velocity.x, 0f, m_CharacterController.velocity.z);
            Vector3 playerVelocityNew = Vector3.zero;

            playerVelocityNew += lateralVelocity + movementDelta;
            playerVelocityNew -= playerVelocityNew.normalized * m_PlayerController.Drag;
            playerVelocityNew = Vector3.ClampMagnitude(playerVelocityNew, moveSpeed);

            m_CharacterController.Move(playerVelocityNew * Time.deltaTime);
        }

		protected override void OnLateUpdateState()
		{

		}
    }
}
