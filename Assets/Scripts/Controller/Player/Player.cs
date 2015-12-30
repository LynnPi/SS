using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Player : MonoBehaviour {
    public float LandingAnimationDuration_ = 0.6f;
    public float LandingCamAniDuration_ = 0.3f;
    public float AcceleratedSpeed = 0.002f;
    public float DecelerationSpeed = 0.01f;

    private float BounceMaxDisatnce_ = 5f;
    private float BounceMinDisatnce_ = 3f;
    private float BounceDuration_ = 0.6f;
    private float LimitedSpeed_ = 0.8f;
    public Rigidbody RigidBody;

    public RelandedHandler MyRelandHandler;
    public DetachHandler MyDetachHandler;
    public PushHandler MyPushHandler;
    public DirectionController DirectionCtrl;

    public Role CurrentRole;

    public Action<int> OnTravelDistanceChanged = delegate { };
    public Action<int> OnTechniquePointChanged = delegate { };
    public Action<int> OnElectricityPointChanged = delegate { };

    public float PushSpeed_ = 0f;
    public float PushSpeed {
        set {
            PushSpeed_ = value;
            OnPushSpeedChanged( PushSpeed_ );

        }
        get {
            return PushSpeed_;
        }
    }

    private void OnPushSpeedChanged( float value ) {
        CurrentRole.RoleAnimator.SetFloat( "Speed", value );
    }

    private float CurrentTravelDistance_;
    public float CurrentTravelDistance {
        get {
            return CurrentTravelDistance_;
        }
        set {
            CurrentTravelDistance_ = value;
            OnTravelDistanceChanged( Mathf.FloorToInt( CurrentTravelDistance_ ) );
        }
    }

    public PlayerStateMachine FSM_;
    public PlayerStateMachine FSM {
        get {
            if( FSM_ == null ) {
                FSM_ = gameObject.AddComponent<PlayerStateMachine>();
                FSM_.SetContext( this );
            }
            return FSM_;
        }
    }

    public static Player CreateInQuest() {
        GameObject attach = new GameObject( "__Player(Quest)" );
        Player instance = attach.AddComponent<Player>();
        instance.CurrentRole = Role.Create( instance.GetRoleModleName(), attach.transform );
        instance.DirectionCtrl = attach.AddComponent<DirectionController>();
        instance.DirectionCtrl.Active = true;
        return instance;
    }

    public static Player CreateInExplore() {
        //GameObject attach = new GameObject( "__Player(Explore)" );
        var role = Role.Create( "Thesurvivors_01", null );
        var attach = role.gameObject;
        Player instance = attach.AddComponent<Player>();
        instance.transform.position = LevelGenerator.Instance.CurrentWreckage.StopPoint.transform.position;
        instance.transform.localEulerAngles = LevelGenerator.Instance.CurrentWreckage.StopPoint.transform.localEulerAngles;
        instance.CurrentRole = role;
        instance.DirectionCtrl = attach.AddComponent<DirectionController>();
        instance.MyDetachHandler = attach.AddComponent<DetachHandler>();
        instance.MyRelandHandler = attach.AddComponent<RelandedHandler>();
        instance.MyPushHandler = attach.AddComponent<PushHandler>();
        instance.RigidBody = attach.GetComponent<Rigidbody>();
        instance.RigidBody.useGravity = false;
        instance.RigidBody.drag = 0.8f;
        return instance;
    }

    private string GetRoleModleName() {
        return "Thesurvivors_01";
    }

    void Start() {
        CubeMapGenerator.Instance.UpdateCubeMap();
    }

    public void PushPlayer() {
        // 加速控制第一版 根据 v = at 来计算瞬时的速率
        /*PushSpeed += AcceleratedSpeed;
        if( PushSpeed >= 0.5f ) {
            PushSpeed = 0.5f;
        }
        else {
            Camera.main.fieldOfView = Mathf.Lerp( CameraManager.Instance.MainCamera.fieldOfView, 60, PushSpeed );
        }

        Vector3 transition = CameraManager.Instance.MainCamera.transform.forward * PushSpeed;
        transform.Translate( transition, Space.World );
        CurrentTravelDistance += PushSpeed;
        */

        // TODO: 空气阻力和推力，要走配置
        this.RigidBody.drag = 0.8f;
        this.RigidBody.AddForce( transform.forward * 8 );
        PushSpeed = this.RigidBody.velocity.magnitude;
        CurrentTravelDistance += PushSpeed * Time.fixedDeltaTime;
        //CurrentRole.RoleAnimator.SetFloat( "Speed", this.RigidBody.velocity.magnitude );
        CameraManager.Instance.MainCamera.fieldOfView = Mathf.Lerp( 55, 60, this.RigidBody.velocity.magnitude * 0.1f );
        CameraManager.Instance.RoleCamera.fieldOfView = Mathf.Lerp( 55, 60, this.RigidBody.velocity.magnitude * 0.1f );
    }

    public void DecelerateMoveRole() {
        /*PushSpeed -= DecelerationSpeed;
        if( PushSpeed <= 0 ) {
            FSM_.SendEvent( "PauseLaunch" );
            return;
        }

        Camera.main.fieldOfView = Mathf.Lerp( CameraManager.Instance.MainCamera.fieldOfView, 55, PushSpeed );
        var transition = CameraManager.Instance.MainCamera.transform.forward * PushSpeed;
        transform.Translate( transition, Space.World );
        CurrentTravelDistance += PushSpeed;
        */
        this.RigidBody.drag = 1.6f;
        PushSpeed = this.RigidBody.velocity.magnitude;
        CurrentTravelDistance += PushSpeed * Time.fixedDeltaTime;
        //CurrentRole.RoleAnimator.SetFloat( "Speed", this.RigidBody.velocity.magnitude );
        CameraManager.Instance.MainCamera.fieldOfView = Mathf.Lerp( 55, 60, this.RigidBody.velocity.magnitude * 0.1f );
        CameraManager.Instance.RoleCamera.fieldOfView = Mathf.Lerp( 55, 60, this.RigidBody.velocity.magnitude * 0.1f );
        if( this.RigidBody.velocity.magnitude <= 2.0f ) {
            //CurrentRole.RoleAnimator.SetFloat( "Speed", 0 );
            FSM_.SendEvent( "PauseLaunch" );
            return;
        }
    }

    public IEnumerator DoLandingAction( Action callback = null ) {
        Debugger.Log( "DoLandingAction: " + Time.frameCount );
        CurrentRole.PlayAnimation( Role.AnimState.Ready_To_Standard );

        if( LevelGenerator.Instance.CurrentWreckage == null ) {
            Debugger.LogError( "DoLandingAction(), target Wreckage is null!" );
            yield break;
        }
        BaseWreckage currentWreckage = LevelGenerator.Instance.CurrentWreckage;
        StartCoroutine( CorrectFace( currentWreckage.transform ) );

        Vector3 landedPoint = currentWreckage.StopPoint.transform.position;

        RaycastHit[] hits;
        hits = Physics.RaycastAll( transform.position, -currentWreckage.transform.up, 10f );
        if( hits.Length != 0 ) {
            for( int i = 0; i < hits.Length; i++ ) {
                if( hits[i].transform.parent.name == Level.BLOCKED_ROOT_NAME ) {
                    landedPoint = hits[i].point;
                    break;
                }
            }
        }

        yield return StartCoroutine( LandMove( landedPoint ) );
        yield return StartCoroutine( LandedCameraAnim() );
        OnLandedAnimationFinished();
        if( callback != null ) {
            callback();
        }
    }

    private IEnumerator LandMove( Vector3 targetPos ) {
        Vector3 oldPos = transform.position;
        float deltaTime = 0;
        while( deltaTime < LandingAnimationDuration_ ) {
            deltaTime += Time.fixedDeltaTime;
            transform.position = Vector3.Lerp( oldPos, targetPos, deltaTime / LandingAnimationDuration_ );
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator LandedCameraAnim() {
        Quaternion lookAtQua = Quaternion.LookRotation( this.transform.forward, this.transform.up );
        var oldLookAt = CameraManager.Instance.FollowController.transform.rotation;
        float deltaTime = 0;
        while( deltaTime <= LandingCamAniDuration_ ) {
            deltaTime += Time.fixedDeltaTime;
            CameraManager.Instance.FollowController.transform.rotation = Quaternion.Lerp( oldLookAt, lookAtQua, deltaTime / LandingCamAniDuration_ );
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnLandedAnimationFinished() {
        Debugger.Log( "着陆动画播放完毕！" );
        //摄像机进入跟随状态
        CameraManager.Instance.FollowTarget( transform );
        FSM.SendEvent( "Landing" );
    }

    private IEnumerator CorrectFace( Transform target ) {
        Vector3 dirInPlane = Vector3.ProjectOnPlane( this.transform.forward, target.transform.up );
        Quaternion lookAtQua = Quaternion.LookRotation( dirInPlane, target.transform.up );
        Quaternion hitPosRotation = this.transform.rotation;
        float deltaTime = 0;
        while( deltaTime <= LandingAnimationDuration_ ) {
            deltaTime += Time.fixedDeltaTime;
            this.transform.rotation = Quaternion.Lerp( hitPosRotation, lookAtQua, deltaTime / LandingAnimationDuration_ );
            yield return new WaitForFixedUpdate();
        }

    }

    public void OnBlockedOff() {
        Debugger.Log( "OnBlockedOff..." );
        // 碰撞到残骸的之后，position的控制权递交给transform
        RigidBody.velocity = Vector3.zero;
        BounceEffect();
    }

    private void BounceEffect() {
        float bounceDistance = 0f;
        bounceDistance = PushSpeed >= LimitedSpeed_ ? BounceMaxDisatnce_ : BounceMinDisatnce_;
        PushSpeed = 0f;
        Vector3 to = CameraManager.Instance.MainCamera.transform.forward * (-bounceDistance) + transform.position;
        LeanTween.move( gameObject, to, BounceDuration_ ).
            setOnComplete( OnBounceFinished ).setEase( LeanTweenType.easeOutCirc );
    }

    private void OnBounceFinished() {
        FSM.SendEvent( "BlockedFinished" );
    }
}
