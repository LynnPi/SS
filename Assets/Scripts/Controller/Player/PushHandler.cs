using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handle the player's pushing.
/// </summary>
public class PushHandler : MonoBehaviour {
    private Player Controller_;
    private Player Controller {
        get {
            if( Controller_ == null ) {
                Controller_ = InitController();
            }
            return Controller_;
        }
    }

    private PointerChecker Handler_;
    public PointerChecker Handler {
        get {
            if( Handler_ == null ) {
                InitHandler();
            }
            return Handler_;
        }
    }

    private bool Active_;
    public bool Active {
        get {
            return Active_;
        }
        set {
            Active_ = value;
            SetActive( Active_ );
        }
    }

    private Player InitController() {
        return GetComponent<Player>();
    }

    private void InitHandler() {
        Transform uiRoot = UIManager.Instance.PanelCanvas.transform;
        GameObject go = uiRoot.FindChild( "ExplorePanel/Button_RoleThruster" ).gameObject;

        //PointerChecker pointer = go.GetComponent<PointerChecker>();

        if( Handler_ == null ) {//First step into game.
            Handler_ = go.AddComponent<PointerChecker>();
            Handler_.OnPointerDownCallback += OnPush;
            Handler_.OnPointerUpCallback += OnPushStop;
        }
    }

    private void OnPush() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        if( !Active ) return;
        Controller.FSM.SendEvent( "Pushing" );
    }

    private void OnPushStop() {
        if( !Active ) return;
        Controller.FSM.SendEvent( "Decelerate" );
    }

    private void SetActive(bool active) {
        Handler.SetActive(active);
    }

    private void OnDestroy() {
        Handler_.OnPointerDownCallback -= OnPush;
        Handler_.OnPointerUpCallback -= OnPushStop;
    }
}
