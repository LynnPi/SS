
public class PrepareLandingState : FiniteState {
    public override void OnEnter( object context ) {
        Player owner = context as Player;
        owner.MyPushHandler.Active = false;
        owner.StartCoroutine( owner.DoLandingAction() );
    }
}