using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestController : MonoBehaviour {
    public static QuestController Instance { get; private set; }
    private Player Player_;
    private GameObject Capsule_;

    public Player CurrentPlayer {
        get {
            return Player_;
        }
    }

    private void Init() {
        SetLights();
        CreateCapsule();
        Player_ = Player.CreateInQuest();
        Player_.FSM.SetContext( Player_ );
        CameraManager.Instance.SetCameraView_WhenInQuestScene();
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Init();
    }

    private void SetLights() {
        GameObject lightRoot = SceneManager.Instance.SceneRoot.FindChild( "Lights" ).gameObject;
        lightRoot.SetActive( false );
    }

    void OnDestroy() {
        if( Player_ )
            Destroy( Player_.gameObject );
        if( Capsule_ )
            Destroy( Capsule_ );
    }

    public List<string> GetAllLevels() {
        List<string> result = new List<string>();
        List<AssetCache> levelChches = AssetBundleLoader.Instance.AssetCacheList[AssetType.Level];
        for( int i = 0; i < levelChches.Count; i++ ) {
            result.Add( levelChches[i].AssetName );
        }
        return result;
    }  
    
    private void CreateCapsule() {
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.BuiltIn, "Model/Capsule" ) as GameObject;
        Capsule_ = Utility.CommonInstantiate( prefab, null );
    }

}
