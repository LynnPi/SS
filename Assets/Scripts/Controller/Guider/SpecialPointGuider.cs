using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpecialPointGuider : MonoBehaviour {
    public enum State { Outside, Inside }
  
    public ExploreGuideHelper Helper;
    public RectTransform OutsideUI;
    public RectTransform InsideUI;
    public Transform UIPairRoot;
    private string OutsidePrefabName_;
    private string InsidePrefabName_;

    private float CanvasWidth_ = 720f;
    private Transform DirectionPointer_;
    private Transform GuideOrigin_;
    private Transform GuideTarget_;
    private Text OutsideDistanceLabel_;
    private Text InsideDistanceLabel_;
    private string Name_;

    private bool Active_;
    public bool Active {
        get {
            return Active_;
        }
        set {
            Active_ = value;
            UIPairRoot.gameObject.SetActive( Active_ );
        }
    }

    private State CurrentState_;
    public State CurrentState {
        get {
            return CurrentState_;
        }
        set {
            CurrentState_ = value;
        }
    }

    private void Start() {
        InitUI();
    }

    private void Update() {
        if( !Active_ )
            return;

        CurrentState = CheckTargetIsInScreen() ? State.Inside : State.Outside;

        if( CurrentState_ == State.Outside ) {
            InsideUI.gameObject.SetActive( false );
            OutsideUI.gameObject.SetActive( true );
            UpdatePointerDirection();
            CorrectUIPositionWhenInside();
            CorrectUIPositionWhenOutside();       
        }
        else if( CurrentState_ == State.Inside ) {
            if( CheckTargetIsBehind() ) {//when target is behind, hide the guider.
                InsideUI.gameObject.SetActive( false );
                OutsideUI.gameObject.SetActive( false );
            }
            else {
                InsideUI.gameObject.SetActive( true );
                OutsideUI.gameObject.SetActive( false );
                CorrectUIPositionWhenInside();
            }          
        }
        else { }

        UpdateDistanceToTarget();
    }

    private void OnDestroy() {
        if( UIPairRoot )
            Destroy( UIPairRoot.gameObject );
    }

    private void InitUI() {
        GameObject uiPrefabOutside =
            AssetBundleLoader.Instance.GetAsset( AssetType.UI, OutsidePrefabName_ ) as GameObject;
        OutsideUI = Utility.CommonInstantiate( uiPrefabOutside, UIPairRoot ).GetComponent<RectTransform>();

        GameObject uiPrefabInside =
            AssetBundleLoader.Instance.GetAsset( AssetType.UI, InsidePrefabName_ ) as GameObject;
        InsideUI = Utility.CommonInstantiate( uiPrefabInside, UIPairRoot ).GetComponent<RectTransform>();

        DirectionPointer_ = OutsideUI.transform.FindChild( "Image" );
        OutsideDistanceLabel_ = OutsideUI.transform.FindChild( "Text" ).GetComponent<Text>();
        InsideDistanceLabel_ = InsideUI.transform.FindChild( "Text" ).GetComponent<Text>();
    }

    private bool CheckTargetIsInScreen() {
        bool result = false;
        Camera camera = CameraManager.Instance.MainCamera;
        Vector3 screenPos = camera.WorldToScreenPoint( GuideTarget_.position );
        if( screenPos.x < Screen.width && screenPos.x > 0 
            && screenPos.y < Screen.height && screenPos.y > 0f ) {
            result = true;
        }
        return result;
    }

    private void CorrectUIPositionWhenInside() {
        InsideUI.parent.name = string.Format( "{0}_{1}", Name_, "Inside" );
        Utility.ScenePositionToUIPosition(
               CameraManager.Instance.MainCamera,
               CameraManager.Instance.UICamera,
               GuideTarget_.position,
               InsideUI.transform );
    }
    
    private void CorrectUIPositionWhenOutside() {
        float offsetX = 52f;
        Vector3 currentPos = InsideUI.transform.localPosition;
        if( currentPos.x>= 0 ) {
            currentPos.x = (CanvasWidth_ / 2) - offsetX;
        }
        else {
            currentPos.x = -(CanvasWidth_ / 2) + offsetX;
        }
        OutsideUI.transform.localPosition = currentPos;
        Helper.SortOutsideGuiderOnVertical();
        
    }

    private bool CheckTargetIsBehind() {
        bool isBehand = false;
        Vector3 directionInScene = GuideTarget_.position - GuideOrigin_.position;
        //when two vector's dot is negative, the angle between is larger than 90.
        if( Vector3.Dot( directionInScene, GuideOrigin_.forward ) < 0 ) {
            isBehand = true;
        }
        return isBehand;
    }

    private void UpdatePointerDirection() {
        Camera camera = CameraManager.Instance.MainCamera;
        Vector3 scenePosOfTarget = camera.WorldToScreenPoint( GuideTarget_.position );
        Vector3 scenePosOfOrigin = camera.WorldToScreenPoint( GuideOrigin_.position );
        Vector2 dirProjectOnScreen = (Vector2)scenePosOfTarget - (Vector2)scenePosOfOrigin;

        Vector3 directionInScene = GuideTarget_.position - GuideOrigin_.position;
        

        if( Application.isEditor ) {//For test in editor.
            Debug.DrawRay( GuideOrigin_.position, directionInScene, Color.red, 0.01f );
        }

        float rotateAngle = Vector3.Angle( dirProjectOnScreen, Vector3.right );
        if( scenePosOfTarget.y <= scenePosOfOrigin.y ) {
            rotateAngle = 180f + Vector3.Angle( dirProjectOnScreen, Vector3.left );
        }
        DirectionPointer_.localEulerAngles = Vector3.forward * rotateAngle;
    }

    private void UpdateDistanceToTarget() {
        Text label = CurrentState_ == State.Outside ? OutsideDistanceLabel_ : InsideDistanceLabel_;
        int currtDistance = (int)Vector3.Distance( GuideOrigin_.position, GuideTarget_.position );
        label.text = string.Format( "{0}m", currtDistance );
    }

    public void Config( string outsidePrefabName, string insidePrefabName ) {
        OutsidePrefabName_ = outsidePrefabName;
        InsidePrefabName_ = insidePrefabName;
    }

    public static SpecialPointGuider Create( ExploreGuideHelper helper, string name, Transform guideTarget, Transform guideOrigin, Transform parent ) {
        GameObject attach = new GameObject( name );
        attach.transform.SetParent( parent );
        SpecialPointGuider instance = attach.AddComponent<SpecialPointGuider>();
        GameObject pairRoot = new GameObject( name );
        pairRoot.transform.SetParent( UIManager.Instance.SceneUICanvas.transform );
        pairRoot.transform.localPosition = Vector3.zero;
        pairRoot.transform.localScale = Vector3.one;
        instance.UIPairRoot = pairRoot.transform;
        instance.GuideTarget_ = guideTarget;
        instance.GuideOrigin_ = guideOrigin;
        instance.Helper = helper;
        instance.Name_ = name;
        return instance;
    }
}
