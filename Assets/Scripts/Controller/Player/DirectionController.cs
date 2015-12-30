using UnityEngine;
using System.Collections;

public class DirectionController : MonoBehaviour {
    public float TouchSensitivity_ = 0.01f;
    private Vector3 LastMousePos_ = Vector3.zero;
    private GameObject Aim_;

    private float MaxRotateY_ = 45f;
    private float RotateYReturnDuration_ = 0.5f;

    [SerializeField]
    private float RotatedY_;
    private float RotatedX_;
    private bool Active_;
    public bool Active {
        get {
            return Active_;
        }
        set {
            Active_ = value;
            Reset();
        }
    }

    private void AimFollow() {
        Vector3 pos = CameraManager.Instance.MainCamera.WorldToViewportPoint( transform.position );
        Aim_.transform.position = CameraManager.Instance.UICamera.ViewportToWorldPoint( pos );
        pos = Aim_.transform.localPosition;
        pos.z = 0f;
        Aim_.transform.localPosition = pos + Vector3.up * 400f;
    }

    void Awake() {
        Aim_ = UIManager.Instance.CreateUI( "Aim", UIManager.Instance.SceneUICanvas.gameObject );

#if UNITY_EDITOR || UNITY_WEBPLAYER
        TouchSensitivity_ = 0.04f;
#else
        TouchSensitivity_ = 0.05f;
#endif
    }

    void Update() {
        if( !Active )
            return;

        if(SceneManager.Instance.CurrentScene == SceneType.Explore ) {
            if( ExploreController.Instance.IsPackageOpen || ExploreController.Instance.IsGameOver ) {
                return;
            }
        }

        Player currentPlayer = SceneManager.Instance.CurrentScene == SceneType.Explore ?
           ExploreController.Instance.CurrentPlayer :
           QuestController.Instance.CurrentPlayer;

        if( Input.GetMouseButton( 0 ) ) {
            if( LastMousePos_ == Vector3.zero ) {
                LastMousePos_ = Input.mousePosition;
                return;
            }

            if( Input.mousePosition == LastMousePos_ ) {
                return;
            }

            Transform contrlTras = SceneManager.Instance.CurrentScene == SceneType.Explore ?
                currentPlayer.transform:
                CameraManager.Instance.ViewPosController;

            Vector2 deltaPos = (Input.mousePosition - LastMousePos_) * TouchSensitivity_;
            if( deltaPos.x != 0 ) {
                if(SceneManager.Instance.CurrentScene == SceneType.Quest ) {
                    if( RotatedX_ + deltaPos.x <= 18f && RotatedX_ + deltaPos.x >= -18f ) {
                        contrlTras.Rotate( Vector3.up, deltaPos.x );
                        RotatedX_ += deltaPos.x;
                    }
                }
                else {
                    contrlTras.Rotate( Vector3.up, deltaPos.x );
                    RotatedX_ += deltaPos.x;
                }
            
            }

            if( currentPlayer.FSM.CurrentState != null) {
                if( (currentPlayer.FSM.CurrentState.Name == typeof( LaunchPauseState ).Name
                || currentPlayer.FSM.CurrentState.Name == typeof( PrepareLaunchState ).Name) ) {
                    if( deltaPos.y != 0 ) {
                        contrlTras.Rotate( Vector3.right, -deltaPos.y );
                    }
                }
                else if( currentPlayer.FSM.CurrentState.Name == typeof( LandedState ).Name ) {
                    if( deltaPos.y != 0 ) {
                        if( deltaPos.y >= 0 ) {
                            if( (RotatedY_ + deltaPos.y) <= MaxRotateY_ ) {                            
                                contrlTras.Rotate( Vector3.right, -deltaPos.y );
                                RotatedY_ = (RotatedY_ + deltaPos.y) > MaxRotateY_ ? MaxRotateY_ : RotatedY_ + deltaPos.y;
                            }
                        }
                        else {
                            if( (RotatedY_ + deltaPos.y) >= -MaxRotateY_ ) {                               
                                contrlTras.Rotate( Vector3.right, -deltaPos.y );
                                RotatedY_ = (RotatedY_ + deltaPos.y) < -MaxRotateY_ ? -MaxRotateY_ : RotatedY_ + deltaPos.y;
                            }
                        }
                    }
                }
                else { }                   
            }

            if(SceneManager.Instance.CurrentScene == SceneType.Explore ) {
                // 同步角色的旋转到摄像机
                CameraManager.Instance.FollowController.transform.localRotation = contrlTras.rotation;
            }

            LastMousePos_ = Input.mousePosition;
        }

        if( Input.GetMouseButtonUp( 0 ) ) {
            LastMousePos_ = Vector3.zero;

            if( currentPlayer && currentPlayer.FSM.CurrentStateName == typeof( LandedState ).Name ) {
                StartCoroutine( LerpRotateY() );
            }
            return;
        }

        if( SceneManager.Instance.CurrentScene == SceneType.Explore ) {
            AimFollow();
        }
    }

    //public void RecordLastQuaternion() {
    //    LastQuaternion_ = ExploreController.Instance.CurrentPlayer.transform.rotation;
    //}

    public void Reset() {
        if( SceneManager.Instance.CurrentScene == SceneType.Explore ) {
            // Todo  继续重构
            if( ExploreController.Instance.CurrentPlayer.FSM.CurrentStateName == typeof( LandedState ).Name
                || ExploreController.Instance.CurrentPlayer.FSM.CurrentStateName == typeof( LaunchState ).Name ) {
                Aim_.SetActive( false );
            }
            else {
                Aim_.SetActive( true );
            }
        }
        else {
            Aim_.SetActive( false );
        }
        LastMousePos_ = Vector3.zero;
    }


    private IEnumerator LerpRotateY() {      
        float deltaYPerFrame = Time.fixedDeltaTime / RotateYReturnDuration_ * Mathf.Abs( RotatedY_ );
        while( RotatedY_ != 0) {
            float delta;
            if(RotatedY_ > 0 ) {
                delta = Mathf.Abs( RotatedY_ ) <= deltaYPerFrame ? Mathf.Abs( RotatedY_ ) : deltaYPerFrame;
                RotatedY_ -= delta;
                ExploreController.Instance.CurrentPlayer.transform.Rotate( Vector3.right, delta );
            }
            else {
                delta = Mathf.Abs( RotatedY_ ) <= deltaYPerFrame ? Mathf.Abs( RotatedY_ ) : deltaYPerFrame;
                RotatedY_ += delta;
                ExploreController.Instance.CurrentPlayer.transform.Rotate( Vector3.right, -delta );
            }

            yield return new WaitForFixedUpdate();
        }
    }

    void OnDestroy() {
        Destroy( Aim_ );
    }
}
