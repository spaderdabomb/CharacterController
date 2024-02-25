
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame.StateMachine
{
	[DefaultExecutionOrder(25)]
	public sealed class StateController : MonoBehaviour
	{
		/*
		 *  Properties
		 */

		/// <summary>
		/// The inital states of the state machine, these are put into <see cref="ActiveStates"/> when
		/// the <see cref="Awake"/> function gets called.
		/// </summary>
		[field: SerializeField]
		public List<State> InitialStates { get; private set; } = new();

		/// <summary>
		/// The active states of the state machine.
		/// </summary>
		[field: SerializeField]
		public List<State> ActiveStates { get; private set; } = new();

		/// <summary>
		/// The states that are currently inactive and aren't running.
		/// </summary>
		[field: SerializeField]
		public List<State> InactiveStates { get; private set; } = new();

		/*
		 *  Functions
		 */

		#region Unity Functions

		private void Awake()
		{
			foreach (State state in InitialStates)
			{
				state.InitializeState();
				ActiveStates.Add(state);
			}

			foreach (State state in InactiveStates)
			{
				state.InitializeState();
			}

			foreach (State state in ActiveStates)
			{
				// All states have been initialized, so we can activate the ones in the active list
				state.StartState();
			}
		}

		#endregion

		#region State Controller Functions

		/// <summary>
		/// <para>Sets the give state to active, this'll remove it from the inactive list and add it to the active list.</para>
		/// <para>This'll also then run the <see cref="State.StartState"/> function.</para>
		/// </summary>
		/// <typeparam name="T">The type of state we wish to set active</typeparam>
		/// <returns>True if the state was successfully set active.</returns>
		public bool SetStateActive<T>() where T : State
		{
			return SetStateActive(typeof(T));
		}

		/// <summary>
		/// <inheritdoc cref="SetStateActive{T}"/>
		/// </summary>
		/// <param name="stateType">The type of state we wish to set active.</param>
		/// <returns>True if the state was successfully set active.</returns>
		public bool SetStateActive(Type stateType)
		{
			if (!CanSetStateActive(stateType))
			{
				return false;
			}

			State state = InactiveStates.Find(s => s.GetType() == stateType);

			if (state == null)
			{
				return false;
			}

			InactiveStates.Remove(state);
			ActiveStates.Add(state);

			state.StartState();
			return true;
		}

		/// <summary>
		/// <inheritdoc cref="SetStateActive{T}"/>
		/// </summary>
		/// <param name="state">The given state class we wish to set active.</param>
		/// <returns>True if the state was successfully set active.</returns>
		public bool SetStateActive(State state)
		{
			if (!CanSetStateActive(state))
			{
				return false;
			}

			InactiveStates.Remove(state);
			ActiveStates.Add(state);

			state.StartState();
			return true;
		}

		public bool SetStateInactive<T>() where T : State
		{
			return SetStateInactive(typeof(T));
		}

		public bool SetStateInactive(Type stateType, bool bTransition = false)
		{
			if (ActiveStates.Find(state => state.GetType() == stateType) == null)
			{
				// state is not active
				return false;
			}

			if (InactiveStates.Find(state => state.GetType() == stateType) != null)
			{
				// state is already inactive
				return false;
			}

			State state = ActiveStates.Find(s => s.GetType() == stateType);

			if (state == null)
			{
				return false;
			}

			ActiveStates.Remove(state);
			InactiveStates.Add(state);

			state.StopState(bTransition);
			return true;
		}

		public bool SetStateInactive(State state, bool bTransition = false)
		{
			if (ActiveStates.Find(s => s == state) == null)
			{
				// state is not active
				return false;
			}

			if (InactiveStates.Find(s => s == state) != null)
			{
				// state is already inactive
				return false;
			}

			ActiveStates.Remove(state);
			InactiveStates.Add(state);

			state.StopState(bTransition);
			return true;
		}

		public bool TransitionState<T>(State oldState) where T : State
		{
			return TransitionState(oldState, typeof(T));
		}

		public bool TransitionState<T>(Type newState) where T : State
		{
			return TransitionState(typeof(T), newState);
		}

		public bool TransitionState(Type oldState, Type newState)
		{
			return SetStateInactive(oldState, true) && SetStateActive(newState);
		}

		public bool TransitionState(State oldState, Type newState)
		{
			return SetStateInactive(oldState, true) && SetStateActive(newState);
		}

		#endregion

		#region State Controller Helper Functions

		public bool IsStateActive<T>() where T : State
		{
			return IsStateActive(typeof(T));
		}

		public bool IsStateActive(Type stateType)
		{
			return ActiveStates.Find(state => state.GetType() == stateType) != null;
		}

		public bool IsStateActive(State state)
		{
			return ActiveStates.Find(s => s == state) != null;
		}

		public bool IsStateInactive<T>() where T : State
		{
			return IsStateInactive(typeof(T));
		}

		public bool IsStateInactive(Type stateType)
		{
			return InactiveStates.Find(state => state.GetType() == stateType) != null;
		}

		public bool IsStateInactive(State state)
		{
			return InactiveStates.Find(s => s == state) != null;
		}

		public bool IsStateOrAnySubclassActive<T>() where T : State
		{
			return IsStateOrAnySubclassActive(typeof(T));
		}

		public bool IsStateOrAnySubclassActive(Type stateType)
		{
			return ActiveStates.Find(state => state.GetType().IsSubclassOf(stateType)) != null;
		}

		public bool IsStateOrAnySubclassActive(State state)
		{
			return ActiveStates.Find(s => s.GetType().IsSubclassOf(state.GetType())) != null;
		}

		public bool CanSetStateActive<T>() where T : State
		{
			return CanSetStateActive(typeof(T));
		}

		public bool CanSetStateActive(Type stateType)
		{
			if (ActiveStates.Find(state => state.GetType() == stateType) != null)
			{
				// state is already active
				return false;
			}

			if (InactiveStates.Find(state => state.GetType() == stateType) == null)
			{
				// state does not exist in our inactive states either
				return false;
			}

			foreach (State inactiveState in InactiveStates)
			{
				if (inactiveState.GetType() != stateType)
				{
					continue;
				}

				return inactiveState.CanSetStateActive();
			}

			return false;
		}

		public bool CanSetStateActive(State state)
		{
			if (ActiveStates.Contains(state))
			{
				// state already active
				return false;
			}

			if (!InactiveStates.Contains(state))
			{
				// state does not exist in our inactive states either
				return false;
			}

			foreach (State inactiveState in InactiveStates)
			{
				if (inactiveState != state)
				{
					continue;
				}

				return inactiveState.CanSetStateActive();
			}

			return false;
		}

		#endregion

	#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying)
			{
				// don't do any valiation in play mode
				return;
			}

			if (ActiveStates.Count > 0)
			{
				Debug.LogWarning($"States can only be put in {nameof(InitialStates)} or {nameof(InactiveStates)}", this);
				ActiveStates.Clear();
			}
		}
	#endif
	}
}
