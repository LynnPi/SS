/// <summary>
/// Base Transition
/// </summary>
public class FiniteStateTransition {
    public string EventName;
    public string LastStateName;
    public string NextStateName;

    public FiniteStateTransition( string eventName, string lastStateName, string nextStateName ) {
        EventName = eventName;
        LastStateName = lastStateName;
        NextStateName = nextStateName;
    }
}