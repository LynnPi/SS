using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 残骸
/// </summary>
public class BaseWreckage : MonoBehaviour {
    public List<BaseSource> SourcePointList = new List<BaseSource>();

    public GameObject StopPoint;
    public bool IsEscape;
    private string PickableRangeRootName = "pickable_range";

    private BoxCollider LandedCollider_;
    private BoxCollider LandedCollider {
        get {
            if( LandedCollider_ == null ) {
                LandedCollider_ = GetComponent<BoxCollider>();
            }
            return LandedCollider_;
        }
    }

    public void SetColliderActive(bool active) {
        LandedCollider.enabled = active;
    }

    private void Awake() {
        Transform tran = transform.GetChild( 0 );
        if( tran.name.ToLower().Contains( "wreckage" ) ) {
            tran = tran.FindChild( PickableRangeRootName );
            if( tran != null ) {
                tran.gameObject.AddComponent<RangePickDetector>();
                tran.gameObject.layer = 2;
            }
            else{
                Debugger.LogError( "wreckage must have a child object that named pickable_range!!!" );
            }

        }
        gameObject.layer = 2;
    }

    private float EscapeDelayDuration_ = 2f;
    private void DelayToChangeScene() {
        SceneManager.Instance.EnterScene( SceneType.Quest );
    }

    public virtual void OnTriggerEnter( Collider other ) {
        //Debugger.LogFormat( "角色{0}进入残骸{1}的碰撞盒范围", other.name, gameObject.name );
        Player role = ExploreController.Instance.CurrentPlayer;

        if( IsEscape ) {// support escape point!
            Debugger.Log( "<color=red>enter escape wreckage, return to the scene of quest!</color>" );
            //AudioManager.Instance.PauseMusic();  
            //if( SceneManager.Instance.CurrentScene == SceneType.Explore && nextScene == SceneType.Quest ) {
            //    fadeDuration = 1f;
            //}
            string clipName;
            if( !GlobalConfig.MusicToScene.TryGetValue( SceneType.Quest.ToString(), out clipName ) ) {
                return;
            }
            AudioClip clip = AssetBundleLoader.Instance.GetAsset( AssetType.Audio, clipName ) as AudioClip;
            float fadeDuration = 1.2f;
            AudioManager.Instance.MusicPlayer.FadeSpeed = 1 / fadeDuration;
            AudioManager.Instance.MusicPlayer.FadeOutThreshold = 0f;
            AudioManager.Instance.PlayMusic( clip, 1, true );
            UIManager.Instance.PlayUISound( "Sound/warp" );
            Invoke( "DelayToChangeScene", EscapeDelayDuration_ );
        }

        // 如果当前是处于悬浮状态，则不触发落地操作
        if( role.FSM.CurrentState != null
            && role.FSM.CurrentState.Name == typeof( PrepareLaunchState ).Name )
            return;
        role.RigidBody.velocity = Vector3.zero;
        LevelGenerator.Instance.CurrentWreckage = this;

        role.FSM.SendEvent( "OnEnterCollider" );
    }

    public virtual void OnTriggerExit( Collider other ) { }

    public SpecialPointGuider Guider;

    private void OnDestroy() {
        if( Guider ) {
            Destroy( Guider );
        }

    }
}
