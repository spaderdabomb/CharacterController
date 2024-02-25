
using UnityEngine;

namespace SpaceGame.StateMachine
{
	/// <summary>
	/// <para>
	///		<see cref="State"/> are derrived from <see cref="MonoBehaviour"/> and are used to represent a state in a <see cref="StateController"/>.
	/// </para>
	/// <para>
	///		The <see cref="MonoBehaviour"/> by default is disabled and is only set to active when the <see cref="State"/> is active.
	/// </para>
	/// </summary>
	[RequireComponent(typeof(StateController))]
	[DefaultExecutionOrder(100)]
	public abstract class State : MonoBehaviour
	{
		/*
		 *  Properties
		 */

		/// <summary>
		/// The owning <see cref="StateController"/> that this state is a part of.
		/// </summary>
		protected StateController m_StateController;

		/*
		 *  Functions
		 */

		#region State Controller Called Functions (Cannot be overriden)

		public void InitializeState()
		{
			// the 'enabled = false' shouldn't be needed as the OnValidate should have already set it to false
			// but just in case it is set to true, we set it to false here
			enabled = false;

			m_StateController = GetComponent<StateController>();
			OnInitializeState();
		}

		public void StartState()
		{
			enabled = true;
			OnStartState();
		}

		public void StopState(bool bTransition = false)
		{
			enabled = false;
			OnStopState(bTransition);
		}

		public void Update()
		{
			OnUpdateState();
			OnTickStateConditionUpdate();
		}

		public void FixedUpdate()
		{
			OnFixedUpdateState();
			OnTickStateConditionFixedUpdate();
		}

		public void LateUpdate()
		{
			OnLateUpdateState();
			OnTickStateConditionLateUpdate();
		}

		public bool CanSetStateActive()
		{
			return enabled == false && OnCanSetStateActive();
		}

		#endregion

		#region State Functions (Can be overriden)

		protected virtual void OnInitializeState() { }

		protected virtual void OnStartState() { }

		protected virtual void OnStopState(bool bTransition = false) { }

		protected virtual void OnUpdateState() { }

		protected virtual void OnFixedUpdateState() { }

		protected virtual void OnLateUpdateState() { }

		protected virtual bool OnCanSetStateActive() { return true; }

		protected virtual void OnTickStateConditionUpdate() { }

		protected virtual void OnTickStateConditionFixedUpdate() { }

		protected virtual void OnTickStateConditionLateUpdate() { }

		#endregion

	#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying)
			{
				return;
			}

			if (enabled)
			{
				// States should NEVER be enabled by default
				// The StateController will enable the state when it is active
				enabled = false;
			}
		}
	#endif
	}
}
