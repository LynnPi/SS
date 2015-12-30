
public class DecelerateLaunchState : FiniteState {

    public override void OnEnter( object context ) {
        EffectManager.ChangeRolePackageParticle( false );
    }

    public override void OnFixedUpdate( object context ) {
        Player owner = context as Player;
        owner.DecelerateMoveRole();
    }
}




