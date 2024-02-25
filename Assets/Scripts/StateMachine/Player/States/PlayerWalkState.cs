using System;
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	public class PlayerWalkState : PlayerMovementState
	{
		[SerializeField] private float m_WalkSpeed = 5f;
		[SerializeField] private float m_WalkAcceleration = 0.35f;
        protected override void OnTickStateConditionUpdate()
		{
			base.OnTickStateConditionUpdate();
		}

		protected override void OnUpdateState()
		{
			base.OnUpdateState();

			MovePlayer(m_PlayerController.m_MovementInput, m_WalkSpeed, m_WalkAcceleration);
		}

		protected override bool OnCanSetStateActive()
		{
			return base.OnCanSetStateActive()
				&& m_StateController.IsStateInactive<PlayerSprintState>();
        }
    }
}
