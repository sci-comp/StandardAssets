using System;

namespace Game
{
    /// <summary>
    /// A generic state machine that works exclusively with enum types.
    /// Used to track, manage and transition between different states in game objects.
    /// Maintains current and previous states, and provides event notifications on state changes.
    /// </summary>
    /// <typeparam name="T">The enum type representing possible states</typeparam>
    public class StateMachine<T>(T initialState) where T : struct, Enum
    {
        public T Current { get; private set; } = initialState;
        public T Previous { get; private set; } = initialState;

        public event Action OnStateChange;

        public void ChangeState(T newState)
        {
            if (Equals(newState, Current))
            {
                return;
            }

            Previous = Current;
            Current = newState;
            OnStateChange?.Invoke();
        }
    }
}

