using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpaceSurivior;

/// <summary>
/// 关卡生成器
/// </summary>
public class LevelGenerator : MonoBehaviour {
    public static LevelGenerator Instance { get; private set; }

    public int FirstStepWreckageID = 1;
    /// <summary>
    /// 当前进入的关卡
    /// </summary>
    public Level CurrenEnterLevel { get; set; }

    public BaseWreckage CurrentWreckage { get; set; }

    public List<BaseWreckage> EscapeWreckageList = new List<BaseWreckage>();
    public List<BaseSource> ChestPointList = new List<BaseSource>();

    /// <summary>
    /// 待生成的关卡信息
    /// </summary>
    private List<LevelInfo> NewLevelList_ = new List<LevelInfo>();

    /// <summary>
    /// 废弃的关卡信息
    /// </summary>
    private List<Level> DesertedLevelList_ = new List<Level>();

    /// <summary>
    /// 已生成的关卡
    /// </summary>
    private List<Level> ActiveLevelList_ = new List<Level>();


    public static LevelGenerator Create(Transform parent) {
        GameObject attach = new GameObject( "__LevelGenerator" );
        attach.transform.SetParent( parent );
        attach.transform.eulerAngles = Vector3.zero;
        attach.transform.position = Vector3.zero;
        attach.transform.localScale = Vector3.one;
        return attach.AddComponent<LevelGenerator>();
    }

    private void Awake() {
        Instance = this;
    }

    public IEnumerator Init() {
        string levelName = GameManager.Instance.PickedLevelName;
        Debug.Log("choose the level name: " + levelName );
        LevelInfo info = AssetBundleLoader.Instance.GetAsset( AssetType.Level, levelName ) as LevelInfo;  
        Debugger.Assert( info != null, "加载的level文件为空！");
        NewLevelList_.Add( info );
        yield return StartCoroutine( OnEnvironmentChanged() );
    }

    private IEnumerator OnEnvironmentChanged() {
        //生成新增的
        if( NewLevelList_.Count > 0 ) {
            for( int i = 0; i < NewLevelList_.Count; i++ ) {
                ActiveLevelList_.Add( Level.Create(transform, NewLevelList_[i] ) );
            }
        }
        NewLevelList_.Clear();

        //删除废弃的
        if( DesertedLevelList_.Count > 0 ) {
            for( int i = DesertedLevelList_.Count - 1; i >= 0; i-- ) {
                var item = DesertedLevelList_[i];
                Destroy( item.gameObject );

                if( ActiveLevelList_.Contains( item ) ) {
                    ActiveLevelList_.Remove( item );
                }

                DesertedLevelList_.RemoveAt( i );
            }
        }

        yield return null;
    }
}
