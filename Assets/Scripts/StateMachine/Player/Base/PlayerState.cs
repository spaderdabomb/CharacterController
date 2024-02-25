
using UnityEngine;

namespace SpaceGame.StateMachine.Player
{
	[RequireComponent(typeof(PlayerController))]
	public abstract class PlayerState : State
	{
		protected PlayerController m_PlayerController = null!;

		protected override void OnInitializeState()
		{
			base.OnInitializeState();
			m_PlayerController = GetComponent<PlayerController>();
		}
	}
}
