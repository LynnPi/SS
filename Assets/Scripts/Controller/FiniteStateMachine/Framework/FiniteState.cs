/// <summary>
/// Base State.
/// </summary>
public class FiniteState {
    public string Name {
        get {
            return GetType().Name;
        }
    }

    public virtual void OnEnter( object context ) { }

    public virtual void OnExit( object context ) { }

    public virtual void OnUpdate( object context ) { }

    public virtual void OnFixedUpdate( object context ) { }

    public virtual void OnLateUpdate( object context ) { }
}

