using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 资源点基类
/// </summary>
public class BaseSource : MonoBehaviour {

    public string EffectName {
        get; set;
    }

    public int Point {
        get; set;
    }
    private GameObject ProBarParent_;
    private GameObject ProBarChild_;
    private float Timer_;
    private float DisappearTime_;
    //private float CurrentResValue_ = 100;
    //private float MaxResValue_ = 100;
    private Slider PickUpSlider_;
    private bool IsPicking_ = false;
    public bool IsOpen = false;
    public bool IsPicked = false;
    public bool IsPicking {
        get {
            return IsPicking_;
        }
        set {
            IsPicking_ = value;
        }
    }

    public GameObject Init() {
        gameObject.SetActive( true );
        ProBarParent_ = UIManager.Instance.CreateUI( "pick_progress_bar", UIManager.Instance.SceneUICanvas.gameObject );
        ProBarChild_ = ProBarParent_.transform.FindChild( "Slider_ProBar" ).gameObject;
        Vector3 viewprotPos = CameraManager.Instance.MainCamera.WorldToViewportPoint( transform.position );
        Vector3 worldPos = CameraManager.Instance.UICamera.ViewportToWorldPoint( viewprotPos );
        ProBarParent_.transform.position = worldPos;
        Vector3 localPos = ProBarParent_.transform.localPosition;
        localPos.z = 0f;
        ProBarParent_.transform.localPosition = localPos;
        PickUpSlider_ = ProBarChild_.transform.FindChild("Slider").GetComponent<Slider>();
        ProBarChild_.SetActive( false );
        return ProBarParent_;
    }

    public void Set( float currentResValue, float disappearTime = 2.0f ) {
        //Debugger.Log( "PickUp---Set" );
        //MaxResValue_ = CurrentResValue_ = currentResValue;
        DisappearTime_ = Timer_ = disappearTime;
    }

    public void Picking() {
        if( null == PickUpSlider_ || false == IsPicking ) return;
        Timer_ -= Time.deltaTime;
        if( Timer_ > 0.0f ) {
            PickUpSlider_.value = Timer_ / DisappearTime_;
            //CurrentResValue_ = PickUpSlider_.value * MaxResValue_;
        }
        else {
            Timer_ = DisappearTime_;
            PickUpSlider_.value = 0.0f;
            OnPickUpComplete();
        }
    }

    public void DestroyProBar() {
        Destroy( ProBarParent_ );
    }

    public void OnPickUpComplete() {
        UIManager.Instance.PlayUISound( "Sound/pickup" );
        IsPicking = false;
        IsOpen = false;
        IsPicked = true;
        AddPoint();
        ExploreController.Instance.PickUp.PickUpComplete(this);
        transform.GetChild( 0 ).gameObject.SetActive( false );
        PlayEffect();
        DestroyProBar();
        Destroy( gameObject );
    }

    public void CloseProBar() {
        IsPicking = false;
        IsOpen = false;
        ProBarParent_.SetActive( false );
    }

    public void OpenProBar() {
        IsOpen = true;
        ProBarParent_.SetActive( true );
    }

    private void OnMouseUp() {
        if(IsOpen && false == IsPicking ) {
            IsPicking = true;
            ProBarChild_.SetActive( true );
            ExploreController.Instance.PickUp.BeginPickup( this );
        }
    }

    private void PlayEffect() {
        GameObject prefab =
            AssetBundleLoader.Instance.GetAsset( AssetType.Effect, EffectName ) as GameObject;
        if( prefab ) {
            Utility.CommonInstantiate( prefab, transform.parent, transform.localPosition, transform.localEulerAngles);
        }
    }

    protected virtual void AddPoint() {}

    public SpecialPointGuider Guider;

    private void OnDestroy() {
        if( Guider ) {
            Destroy( Guider );
        }
    }    
}
