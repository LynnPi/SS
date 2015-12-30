using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RelandedHandler : MonoBehaviour {

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
        Transform behaviorTrans = uiRoot.FindChild( "ExplorePanel/Button_Relanded" );
        Button btn = behaviorTrans.GetComponent<Button>();
        btn.onClick.AddListener( OnTriggerRelanded );
        return btn;
    }

    private void OnTriggerRelanded() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        OnReland();
    }

    public void OnReland() {       
        Active = false;
        Handler.transform.SetAsLastSibling();
        Controller.MyDetachHandler.Handler.gameObject.SetActive( false );

        Controller.CurrentRole.PlayAnimation( Role.AnimState.Ready_To_Standard );
        Controller.FSM.SendEvent( "Reland" );
        Controller.StartCoroutine( Controller.DoLandingAction( OnRelandAnimCompleted ) );
    }

    private void OnRelandAnimCompleted() {
        Controller.MyDetachHandler.Handler.gameObject.SetActive( true );
        Controller.MyDetachHandler.Handler.transform.SetAsLastSibling();
        Controller.MyDetachHandler.Active = true;
    }
}
