using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Receive the command of taking off from wreckage and do behavior.
/// </summary>
public class DetachHandler : MonoBehaviour {
    public LeanTweenType TweenType = LeanTweenType.easeInOutQuart;

    public float DetachingSpeed = 0.2f;
    public float DetachingHeight = 1.5f;

    private Player Controller_;
    private Player Controller {
        get {
            if( Controller_ == null ) {
                Controller_ = InitController();
            }
            return Controller_;
        }
    }

    private Button Handler_;
    public Button Handler {
        get {
            if( Handler_ == null ) {
                Handler_ = InitHandler();
            }
            return Handler_;
        }
    }

    public bool Active {
        get {
            return Handler.interactable;
        }
        set {         
            Handler.interactable = value;
        }
    }

    private Player InitController() {
        return transform.GetComponent<Player>();
    }

    private Button InitHandler() {
        Transform uiRoot = UIManager.Instance.PanelCanvas.transform;
        Transform behaviorTrans = uiRoot.FindChild( "ExplorePanel/Button_Float" );
        Button btn = behaviorTrans.GetComponent<Button>();
        btn.onClick.AddListener( OnTriggerDetaching );
        return btn;
    }

    private void OnTriggerDetaching() {
        OnDetaching();
    }

    public void OnDetaching() {
        Active = false;
        Handler.transform.SetAsLastSibling();
        Controller.MyRelandHandler.Handler.gameObject.SetActive(false);

        Controller.CurrentRole.PlayAnimation( Role.AnimState.Standard_To_Ready );
        Vector3 to = transform.position + transform.up * DetachingHeight;
        LeanTween.move( gameObject, to, 1.2f ).setEase( TweenType ).
            setOnComplete( OnDetachAnimationCompleted );
    }

    private void OnDetachAnimationCompleted() {
        Controller.DirectionCtrl.Active = true;
        Controller.MyPushHandler.Active = true;
        Controller.MyRelandHandler.Handler.gameObject.SetActive( true );
        Controller.MyRelandHandler.Handler.transform.SetAsLastSibling();
        Controller.MyRelandHandler.Active = true;
    }

}
