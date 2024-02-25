
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public class PlayerJumpState : PlayerMovementState
	{
		protected override void OnStartState()
		{
			base.OnStartState();
		}

		protected override void OnTickStateConditionFixedUpdate()
		{
			base.OnTickStateConditionFixedUpdate();
		}

		protected override bool OnCanSetStateActive()
		{
			return base.OnCanSetStateActive()
		       && m_PlayerController.IsGrounded()
				&& m_StateController.IsStateInactive<PlayerFallState>();
		}
	}
}
