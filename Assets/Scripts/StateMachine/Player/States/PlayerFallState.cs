
namespace SpaceGame.StateMachine.Player
{
	public class PlayerFallState : PlayerMovementState
	{
		protected override void OnTickStateConditionFixedUpdate()
		{
			base.OnTickStateConditionFixedUpdate();

			if (m_PlayerController.IsGrounded())
			{
				m_StateController.SetStateInactive(this);
			}
		}
	}
}
