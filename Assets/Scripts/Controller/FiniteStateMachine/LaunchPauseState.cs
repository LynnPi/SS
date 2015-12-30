
public class LaunchPauseState : FiniteState {

    public override void OnEnter( object context ) {
        Player owner = context as Player;
        owner.DirectionCtrl.Active = true;
        CameraBreatheAnim.Instance.Enable = true;
        EffectManager.ChangeRolePackageParticle( true );
        ChangeFuelBuringSpeed();
    }

    public override void OnExit( object context ) {
        CameraBreatheAnim.Instance.Enable = false;      
    }

    public override void OnFixedUpdate( object context ) {
        Player owner = context as Player;
        owner.PushSpeed = owner.RigidBody.velocity.magnitude;
    }

    private void ChangeFuelBuringSpeed() {
        ExploreController.Instance.FuelBurningSpeed = PlayerDataCenter.CurrentRoleInfo.LowFuelSpeed;
    }
}