using UnityStandardAssets.Cameras;

public class PrepareLaunchState : FiniteState {
    public override void OnEnter( object context ) {
        CameraManager.Instance.FollowController.CurrUpdateType = 
            AbstractTargetFollower.UpdateType.LateUpdate ;

        CameraManager.Instance.FollowController.MoveSpeed = 3f;

        CameraManager.Instance.StartCoroutine( CameraManager.Instance.ChangeCameraFov( 55f, 1.2f ) );

        CameraBreatheAnim.Instance.Enable = true;
        EffectManager.ChangeRolePackageParticle( true );
        ChangeFuelBuringSpeed();
    }

    public override void OnExit( object context ) {
        CameraManager.Instance.FollowController.CurrUpdateType =
          AbstractTargetFollower.UpdateType.FixedUpdate;

        CameraManager.Instance.FollowController.MoveSpeed = 50f;

        CameraBreatheAnim.Instance.Enable = false;
    }

    private void ChangeFuelBuringSpeed() {
        ExploreController.Instance.FuelBurningSpeed = PlayerDataCenter.CurrentRoleInfo.LowFuelSpeed;
    }
}