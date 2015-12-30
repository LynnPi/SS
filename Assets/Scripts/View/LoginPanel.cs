using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginPanel : UIPanelBehaviour {

    private GameObject LoginEffect_;
    private GameObject LoginEffect {
        get {
            if (LoginEffect_ == null) {
                LoginEffect_ = SceneManager.Instance.SceneRoot.transform.FindChild("CC_login").gameObject;
            }
            return LoginEffect_;
        }
    }

    private GameObject LoginBg_;
    private GameObject LoginBg {
        get {
            if (LoginBg_ == null) {
                LoginBg_ = UIManager.Instance.SceneUICanvas.transform.FindChild("Login_Bg").gameObject;
            }
            return LoginBg_;
        }
    }

    private void OnClickScreen() {
        //Debugger.Log("Click Screen, step into next scene!");
        Invoke( "EnterQuest", 0.2f );
    }

    private void EnterQuest() {
        SceneManager.Instance.EnterScene( SceneType.Quest );
    }

    protected override void OnAwake() {
        Button btn = transform.FindChild("Button_ScreenClick").GetComponent<Button>();
        btn.onClick.AddListener(OnClickScreen);
    }

    protected override void OnShow(params object[] args) {
        LoginBg.SetActive(true);
        LoginEffect.SetActive(true);
    }

    protected override void OnClose() {
        LoginBg.SetActive(false);
        LoginEffect.SetActive(false);
    }
}
