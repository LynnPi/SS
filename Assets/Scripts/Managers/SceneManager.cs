using UnityEngine;
using System.Collections;

public enum SceneType {
    None,
    Login,
    Quest,
    Explore
}

public class SceneManager : MonoBehaviour {
    private static SceneManager Instance_;
    public static SceneManager Instance {
        get {
            if (Instance_ == null) {
                GameObject attach = new GameObject("__SceneManager__");
                Instance_ = attach.AddComponent<SceneManager>();
            }
            return Instance_;
        }
    }

    private Transform SceneRoot_;
    public Transform SceneRoot {
        get {
            if (SceneRoot_ == null) {
                SceneRoot_ = GameObject.Find("SceneRoot").transform;
            }
            return SceneRoot_;
        }
    }


    public SceneType CurrentScene { get; set; }

    private GameObject Controller_;


    private void InitScene() {
        if (Controller_) {
            Destroy(Controller_);
        }

        Controller_ = new GameObject(CurrentScene.ToString());
        Controller_.transform.parent = Instance.transform;
        switch (CurrentScene) {
            case SceneType.Login:
                UIManager.ShowPanel<LoginPanel>();
                break;
            case SceneType.Quest:
                UIManager.ShowPanel<QuestPanel>();
                Controller_.AddComponent<QuestController>();
                break;
            case SceneType.Explore:
                UIManager.ShowPanel<ExplorePanel>();
                Controller_.AddComponent<ExploreController>();
                break;
            default:
                break;
        }
    }

    public void EnterScene(SceneType newScene) {
        if( CurrentScene == newScene )
            return;
       
        ChangeMusicByScene( CurrentScene, newScene );
        CurrentScene = newScene;
        InitScene();      
    }

    
    private void ChangeMusicByScene( SceneType currScene, SceneType nextScene ) {
        string clipName;
        if( !GlobalConfig.MusicToScene.TryGetValue( nextScene.ToString(), out clipName ) ) {
            return;
        }
        AudioClip clip = AssetBundleLoader.Instance.GetAsset( AssetType.Audio, clipName ) as AudioClip;

        float fadeDuration = 1f;//fade with given seconds.
        if( nextScene == SceneType.Login ) {
            fadeDuration = 5f;
        }
        else if( currScene == SceneType.Login && nextScene == SceneType.Quest) {
            fadeDuration = 5f;
        }
        else if( currScene == SceneType.Quest && nextScene == SceneType.Explore ) {
            fadeDuration = 1f;
        }
        else if( currScene == SceneType.Explore && nextScene == SceneType.Quest ) {
            fadeDuration = 1f;
        }

        AudioManager.Instance.MusicPlayer.FadeSpeed = 1 / fadeDuration;
        AudioManager.Instance.PlayMusic( clip, 1, true );
        //if(CurrentScene != SceneType.Explore ) {
        //    AudioManager.Instance.MusicPlayer.FadeSpeed = 1 / fadeDuration;
        //    AudioManager.Instance.PlayMusic( clip, 1, true );
        //}
    }
}