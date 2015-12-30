using UnityEngine;
public class LaunchState : FiniteState {
    public override void OnEnter( object context ) {
        Player owner = context as Player;
        owner.DirectionCtrl.Active = false;
        owner.MyRelandHandler.Active = false;
        owner.MyDetachHandler.Handler.gameObject.SetActive( false );
        EffectManager.ChangeRolePackageParticle( false );
        ChangeFuelBuringSpeed();
    }

    public override void OnFixedUpdate( object context ) {
        Player owner = context as Player;
        if( PlayerDataCenter.CurrentRoleInfo.Fuel <= 0 )
            return;
        owner.PushPlayer();
    }

    private void ChangeFuelBuringSpeed() {
        ExploreController.Instance.FuelBurningSpeed = PlayerDataCenter.CurrentRoleInfo.HighFuelSpeed;
    }
}