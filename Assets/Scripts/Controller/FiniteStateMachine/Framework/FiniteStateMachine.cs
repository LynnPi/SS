using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PalmPioneer.StateMachine {
    [System.Serializable]
    public class FiniteStateMachine : MonoBehaviour {
        private object Context_;
        public string CurrentStateName = "EmptyState";

        private FiniteState CurrentState_;
        public FiniteState CurrentState {
            get {
                return CurrentState_;
            }
            set {
                CurrentState_ = value;
                CurrentStateName = CurrentState_.Name;
            }
        }

        protected FiniteState defaultState;

        private Dictionary<string, FiniteState> allStates_ = new Dictionary<string, FiniteState>();
        private List<FiniteStateTransition> allTransitions_ = new List<FiniteStateTransition>();

        protected virtual void Init() { }

        protected void AddState( FiniteState state ) {
            allStates_.Add( state.Name, state );
        }

        protected void RemoveState( FiniteState state ) {
            allStates_.Remove( state.Name );
        }

        /// <summary>
        /// Add transition
        /// </summary>
        /// <param name="eventName">Event name.</param>
        /// <typeparam name="T1">src state type.</typeparam>
        /// <typeparam name="T2">desk state type.</typeparam>
        protected void AddTransition<T1, T2>( string eventName ) where T1 : FiniteState where T2 : FiniteState {
            string lastStateName = typeof( T1 ).Name;
            string nextStateName = typeof( T2 ).Name;
            FiniteStateTransition transition = new FiniteStateTransition( eventName, lastStateName, nextStateName );
            allTransitions_.Add( transition );
        }

        protected void AddAnyTransition<T>(string eventName ) where T : FiniteState  {
            string nextStateName = typeof( T ).Name;
            foreach( var state in allStates_ ) {
                string lastStateName = state.Value.GetType().Name;
                FiniteStateTransition transition = new FiniteStateTransition( eventName, lastStateName, nextStateName );
                allTransitions_.Add( transition );
            }
        }

        protected void AddTransition( FiniteStateTransition transition ) {
            allTransitions_.Add( transition );
        }

        protected void RemoveTransation( FiniteStateTransition transition ) {
            allTransitions_.Remove( transition );
        }

        public void SetContext( object context ) {
            Context_ = context;
        }

        /// <summary>
        /// Trigger the event to target state.
        /// </summary>
        /// <param name="eventName">.</param>
        public void SendEvent( string triggerName ) {
            for( int i = 0; i < allTransitions_.Count; i++ ) {
                FiniteState nextState;
                if( TryGetNextStateByEventName( allTransitions_[i], triggerName, out nextState ) ) {
                    ChangeState( nextState );
                    break;
                }
            }
            //Debug.LogWarningFormat( "No Such Transition Trigger By The Event [{0}].", eventName );
        }

        private bool TryGetNextStateByEventName( 
            FiniteStateTransition transition, string eventName, out FiniteState state ) {
            state = null;
            if( CurrentState == null || !CurrentState.Name.Equals( transition.LastStateName ) )
                return false;
            if( !eventName.Equals( transition.EventName ) )
                return false;
            return (allStates_.TryGetValue( transition.NextStateName, out state ));
        }

        private void ChangeState( FiniteState newState ) {
            if( CurrentState != null ) {
                CurrentState.OnExit( Context_ );
            }
            else {
                Debug.LogError( "current state is null, fail to exit state!", gameObject );
            }

            CurrentState = newState;

            if( CurrentState != null ) {
                CurrentState.OnEnter( Context_ );
            }
            else {
                Debug.LogError( "current state is null, fail to enter state!", gameObject );
            }
        }

        private void SetDefaultToCurrent() {
            CurrentState = defaultState;
        }

        private void Awake() {
            Init();
            SetDefaultToCurrent();
        }

        private void Update() {
            if( CurrentState != null ) {
                CurrentState.OnUpdate( Context_ );
            }
        }

        private void FixedUpdate() {
            if( CurrentState != null ) {
                CurrentState.OnFixedUpdate( Context_ );
            }
        }

        private void LateUpdate() {
            if( CurrentState != null ) {
                CurrentState.OnLateUpdate( Context_ );
            }
        }
    }
}