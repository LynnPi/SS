using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 加载资源的进度显示器
/// </summary>
public class DownloadProgressDisplayer : MonoBehaviour {

    private Text DownloadText_;
    private Text DownloadText {
        get {
            if( DownloadText_ == null ) {
                DownloadText_ = GameObject.Find( "UIRoot/Canvas-UIPanel/DownloadText" ).GetComponent<Text>();
            }
            return DownloadText_;
        }
    }

    private string MessageText_;
    private string MessageText {
        set {
            MessageText_ = value;
            DownloadText.text = MessageText_;
        }
    }

    public void OnMessage(string msg) {
        MessageText = msg;
    }

    public static DownloadProgressDisplayer Create(Transform parent) {
        DownloadProgressDisplayer instance;
        GameObject attach = new GameObject( "__DownloadProgressDisplayer" );
        attach.transform.SetParent( parent );
        instance = attach.AddComponent<DownloadProgressDisplayer>();
        return instance;
    }

    private void OnDestroy() {
        if( DownloadText_ )
            DownloadText_.gameObject.SetActive( false );
    }
}
