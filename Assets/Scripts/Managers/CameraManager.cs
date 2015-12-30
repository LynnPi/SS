using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance_;
    public static CameraManager Instance {
        get {
            if (Instance_ == null) {
                GameObject attach = new GameObject("__CameraManager__");
                Instance_ = attach.AddComponent<CameraManager>();
            }
            return Instance_;
        }
    }

    private Transform Pivot_;
    public Transform Pivot {
        get {
            if(Pivot_ == null ) {
                Pivot_ = FollowController.transform.FindChild( "Pivot" );
            }
            return Pivot_;
        }
    }

    private Transform ViewPosController_;
    public Transform ViewPosController{
        get {
            if( ViewPosController_ == null ) {
                ViewPosController_ = FollowController.transform.FindChild( "Pivot/ViewPosController" );
            }
            return ViewPosController_;
        }
    }

    private Camera MainCamera_;
    public Camera MainCamera {
        get {
            if (MainCamera_ == null) {
                MainCamera_ = Camera.main;
                if (MainCamera_ == null) {
                    Debugger.LogError("在场景中未找到主相机！");
                }
            }
            return MainCamera_;
        }
    }

    private Camera UICamera_;
    public Camera UICamera {
        get {
            if( UICamera_ == null ) {
                UICamera_ = GameObject.Find( "UICamera" ).GetComponent<Camera>();
            }
            return UICamera_;
        }
    }

    private Camera SceneUICamera_;
    public Camera SceneUICamera {
        get {
            if( SceneUICamera_ == null ) {
                SceneUICamera_ = GameObject.Find( "SceneUICamera" ).GetComponent<Camera>();
            }
            return SceneUICamera_;
        }
    }

    private Camera RoleCamera_;
    public Camera RoleCamera {
        get {
            if( RoleCamera_ == null ) {
                RoleCamera_ = GameObject.Find( "RoleCamera" ).GetComponent<Camera>();
            }
            return RoleCamera_;
        }
    }

    private AutoCam FollowController_;
    public AutoCam FollowController {
        get {
            if( FollowController_  == null ) {
                FollowController_ = MainCamera.transform.parent.GetComponentInParent<AutoCam>();
            }
            return FollowController_;
        }
    }

    #region 公共接口
    public void SetCameraView_WhenInQuestScene() {
        RenderSettings.ambientIntensity = 0.5f;
        FollowController.SetTarget( null );
        FollowController.transform.position = Vector3.zero;
        FollowController.transform.localEulerAngles = Vector3.zero;
        Pivot.localEulerAngles = Vector3.zero;
        Pivot.localPosition = new Vector3( 0f, 1.72f, 7.74f );
        MainCamera.fieldOfView = 45f;
        MainCamera.transform.localPosition = new Vector3( 0f, 0.25f, 2.08f );
        MainCamera.transform.localEulerAngles = new Vector3( 4f, 180f, 0f );

        RoleCamera.fieldOfView = 45f;
        RoleCamera.transform.localPosition = new Vector3( 0f, 0.25f, 2.08f );
        RoleCamera.transform.localEulerAngles = new Vector3( 4f, 180f, 0f );
    }

    public void SetCameraView_WhenInExploreScene() {
        RenderSettings.ambientIntensity = 1.1f;
        FollowController.transform.position = LevelGenerator.Instance.CurrentWreckage.transform.position - Vector3.forward * 32f;
        MainCamera.fieldOfView = 55f;
        Pivot.localEulerAngles = Vector3.zero;
        Pivot.localPosition = Vector3.zero;
        ViewPosController.localEulerAngles = Vector3.zero;
        MainCamera.transform.localPosition = new Vector3( 0f, 5.67f, -9.86f );
        MainCamera.transform.localEulerAngles = new Vector3( 11f, 0f, 0f );

        RoleCamera.fieldOfView = 55f;
        RoleCamera.transform.localPosition = new Vector3( 0f, 5.67f, -9.86f );
        RoleCamera.transform.localEulerAngles = new Vector3( 11f, 0f, 0f );
        UICamera.depth = 10;
        RoleCamera.depth = 9;
    }

    public void FollowTarget( Transform target ) {
        Debugger.Log( "Camera start following!" );
        FollowController.SetTarget( target );
    }

    public IEnumerator ChangeCameraFov(float targetFov, float duration) {
        float originFov = MainCamera.fieldOfView;
        float percent = 0f;
        while( percent < 1f ) {
            percent += Time.fixedDeltaTime / duration; 
            MainCamera.fieldOfView = Mathf.Lerp( originFov, targetFov, percent );
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion
}
