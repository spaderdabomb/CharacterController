
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public class PlayerIdleState : PlayerMovementState
    {
        static readonly int isIdling = Animator.StringToHash("isIdling");

        protected override void OnStartState()
        {
            base.OnStartState();

            m_Animator.SetBool(isIdling, true);
        }
        protected override void OnTickStateConditionUpdate()
        {
            base.OnTickStateConditionUpdate();

            if (m_PlayerController.m_MovementInput != Vector2.zero && m_PlayerController.IsSprinting())
            {
                m_StateController.TransitionState<PlayerSprintState>(this);
            }
            else if (m_PlayerController.m_MovementInput != Vector2.zero)
            {
                m_StateController.TransitionState<PlayerRunState>(this);
            }
        }

        protected override bool OnCanSetStateActive()
        {
            return base.OnCanSetStateActive()
                && m_StateController.IsStateInactive<PlayerWalkState>()
                && m_StateController.IsStateInactive<PlayerRunState>()
                && m_StateController.IsStateInactive<PlayerSprintState>();
        }

        protected override void OnStopState(bool bTransition = false)
        {
            base.OnStopState(bTransition);

            m_Animator.SetBool(isIdling, false);
        }
    }
}
