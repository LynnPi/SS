using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestPanel : UIPanelBehaviour {
    private RectTransform Grid_;

    protected override void OnAwake() {
        Init();
    }

    private void Init() {
        //btn = transform.FindChild("Button_Switch").GetComponent<Button>();
        //btn.onClick.AddListener(OnClickSwitch);
        //btn = transform.FindChild("Button_Return").GetComponent<Button>();
        //btn.onClick.AddListener(OnClickReturn);     

        Grid_ = transform.FindChild( "Scroll Rect/Grid" ) as RectTransform;
        //临时代码
        Grid_.parent.gameObject.SetActive( false );
    }

    void Start() {
        //临时注释
        //ShowQuestList( QuestController.Instance.GetAllLevels() );
    }


    protected override void OnUpdate() {
        if( IsClickScreen() ) {
            GameManager.Instance.PickedLevelName = "1003";
            OnScreenClick();
        }
    }

    private Vector3 PrePos_;
    private bool IsClickScreen() {
        if( Input.GetMouseButtonDown( 0 ) ) {
            PrePos_ = Input.mousePosition;
        }

        if( Input.GetMouseButtonUp( 0 ) ) {
            Vector3 offset = Input.mousePosition - PrePos_;
            if( (Mathf.Abs( offset.x ) < 1 || Mathf.Abs( offset.y ) < 1) &&
                ((Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width) &&
                  Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height) ) {
                return true;
            }
        }

        return false;
    }

    private void OnQuestClick(string text) {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        GameManager.Instance.PickedLevelName = text;
        SceneManager.Instance.EnterScene( SceneType.Explore );
    }

    public void ShowQuestList( List<string> list ) {
        Button btn;
        for( int i = 0; i < list.Count; i++ ) {
            btn = UIManager.Instance.CreateUI( "Button_Level", Grid_.gameObject ).GetComponent<Button>();
            //这里Text必须在这里定义
            Text text = btn.transform.FindChild( "Text" ).GetComponent<Text>();
            text.text = list[i];
            btn.onClick.AddListener( () =>OnQuestClick( text.text ));//这里传值不能是list[i]
        }
    }

    private void OnScreenClick() {
        SceneManager.Instance.EnterScene(SceneType.Explore);
    }

    private void OnClickSwitch() {
        //Debugger.Log("OnClickSwitch()...");
        UIManager.Instance.PlayUISound("click_button");
        SceneManager.Instance.EnterScene(SceneType.Explore);
    }

    private void OnClickReturn() {
        //Debugger.Log("OnClickReturn()...");
        UIManager.Instance.PlayUISound("click_button");
        if(SceneManager.Instance.CurrentScene == SceneType.Quest ) {
            SceneManager.Instance.EnterScene( SceneType.Login );
        }
        else if( SceneManager.Instance.CurrentScene == SceneType.Explore ) {
            SceneManager.Instance.EnterScene( SceneType.Quest );
        }
    }

}
