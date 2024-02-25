
using System;
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public class PlayerRunState : PlayerMovementState
	{
        [SerializeField] private float m_RunSpeed = 10f;
        [SerializeField] private float m_RunAcceleration = 0.5f;

        static readonly int isRunning = Animator.StringToHash("isRunning");

        protected override void OnStartState()
        {
            base.OnStartState();

            m_Animator.SetBool(isRunning, true);
        }

        protected override void OnTickStateConditionUpdate()
		{
			base.OnTickStateConditionUpdate();

            print(m_PlayerController.IsSprinting());
            if (!m_PlayerController.IsMovingLaterally())
            {
                m_StateController.TransitionState<PlayerIdleState>(this);
            }
            else if (m_PlayerController.IsSprinting())
            {
                m_StateController.TransitionState<PlayerSprintState>(this);
            }
        }

        protected override void OnUpdateState()
		{
			base.OnUpdateState();

			MovePlayer(m_PlayerController.m_MovementInput, m_RunSpeed, m_RunAcceleration);
		}

		protected override bool OnCanSetStateActive()
		{
			return base.OnCanSetStateActive()
                && m_StateController.IsStateInactive<PlayerSprintState>()
                && m_StateController.IsStateInactive<PlayerWalkState>();
        }

        protected override void OnStopState(bool bTransition = false)
        {
            base.OnStopState(bTransition);

            m_Animator.SetBool(isRunning, false);
        }
    }
}
