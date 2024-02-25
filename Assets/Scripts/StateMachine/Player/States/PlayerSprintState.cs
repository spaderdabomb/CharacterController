
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public class PlayerSprintState : PlayerMovementState
	{
		[SerializeField] private float m_SprintSpeed = 20f;
		[SerializeField] private float m_SprintAcceleration = 1f;

        static readonly int isSprinting = Animator.StringToHash("isSprinting");

        protected override void OnStartState()
        {
            base.OnStartState();

            m_Animator.SetBool(isSprinting, true);
        }

        protected override void OnTickStateConditionUpdate()
		{
			base.OnTickStateConditionUpdate();

			if (!m_PlayerController.IsMovingLaterally())
			{
				m_StateController.TransitionState<PlayerIdleState>(this);
			}
			else if (!m_PlayerController.IsSprinting())
			{
				m_StateController.TransitionState<PlayerRunState>(this);
			}
		}

		protected override void OnUpdateState()
		{
			base.OnUpdateState();

			MovePlayer(m_PlayerController.m_MovementInput, m_SprintSpeed, m_SprintAcceleration);
		}

		protected override bool OnCanSetStateActive()
		{
			return base.OnCanSetStateActive()
                && m_StateController.IsStateInactive<PlayerWalkState>()
                && m_StateController.IsStateInactive<PlayerRunState>();
        }

        protected override void OnStopState(bool bTransition = false)
        {
            base.OnStopState(bTransition);

            m_Animator.SetBool(isSprinting, false);
        }
    }
}
