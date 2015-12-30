using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandedState : FiniteState {

    public override void OnEnter( object target ) {
        Player owner = target as Player;
        CameraManager.Instance.FollowTarget( owner.transform );
        owner.DirectionCtrl.Active = true;
        owner.MyDetachHandler.Handler.gameObject.SetActive(true);
        owner.MyDetachHandler.Active = true;
        owner.MyDetachHandler.Handler.transform.SetAsLastSibling();
        owner.MyPushHandler.Active = false;
        LevelGenerator.Instance.CurrentWreckage.SetColliderActive( false );
        owner.PushSpeed = 0f;
        owner.CurrentRole.PlayAnimation( Role.AnimState.Ready_To_Standard );
        ExploreController.Instance.PickUp.OnPlayerLanded();
        ExploreController.Instance.CurrentPlayer.DirectionCtrl.Reset();
        CameraManager.Instance.transform.position = ExploreController.Instance.CurrentPlayer.transform.position;
        CameraManager.Instance.FollowController.transform.localRotation = ExploreController.Instance.CurrentPlayer.transform.rotation;
        CameraManager.Instance.StartCoroutine( CameraManager.Instance.ChangeCameraFov( 60f, 1.2f ) );
        EffectManager.ChangeRolePackageParticle( true );
        ChangeBuringSpeed();
    }


    public override void OnExit( object target ) {
        Player owner = target as Player;
        LevelGenerator.Instance.CurrentWreckage.SetColliderActive( true );
        owner.DirectionCtrl.Active = false;
        ExploreController.Instance.PickUp.OnPlayerAway();
        //ExploreController.Instance.OxygenBurningSpeed = PlayerDataCenter.CurrentRoleInfo.OxygenSpeed;
    }

    private void ChangeBuringSpeed() {
        ExploreController.Instance.FuelBurningSpeed = 0;
        //ExploreController.Instance.OxygenBurningSpeed = 0;
    }
}
