using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏总控制器
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    private DownloadProgressDisplayer DownloadProgressDisplayer_;

    public string PickedLevelName;

    void Awake() {
        Instance = this;
    }

    private IEnumerator Start() {

        Debugger.EnableLog = true;
        Application.targetFrameRate = 45;
        GlobalConfig.LoadXml();
        //AssetConfig.ReadBundleConfig();
        GlobalConfig.ReadConfig();
        //AssetConfig.ReadAudioConfig();
        GameObject loginText = GameObject.Find( "SceneRoot/CC_login/Cc04" );
        loginText.SetActive( false );
        DownloadProgressDisplayer_ = DownloadProgressDisplayer.Create(transform);
        AssetBundleLoader.Instance.LoadMessageUpdateCallback = DownloadProgressDisplayer_.OnMessage;

        yield return StartCoroutine( AssetBundleLoader.Instance.Preload() );

        if(AssetBundleLoader.Instance.LoadErrors.Count > 0 ) {
            Debugger.LogErrorFormat("Download error count: " + AssetBundleLoader.Instance.LoadErrors.Count );
            yield break;
        }

        Destroy( DownloadProgressDisplayer_.gameObject);
        SceneManager.Instance.EnterScene( SceneType.Login );
        loginText.SetActive( true );
    }
}
