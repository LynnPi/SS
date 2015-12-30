using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpaceSurivior;

/// <summary>
/// Client Level.
/// </summary>
public class Level : MonoBehaviour {
    public static readonly string BLOCKED_ROOT_NAME = "blocked";
    public LevelInfo Info { get; private set; }

    private BaseWreckage BirthWreckage_;
    private BaseWreckage BirthWreckage {
        get {
            return BirthWreckage_;
        }
        set {
            BirthWreckage_ = value;
            LevelGenerator.Instance.CurrentWreckage = BirthWreckage_;
        }
    }

    public static Level Create( Transform parent, LevelInfo info ) {
        Debugger.Assert( info != null, "Init level, level info is null!" );
        string name = string.Format( "level_{0}", info.ID );
        GameObject attach = new GameObject( name );
        attach.transform.SetParent( parent );
        Level level = attach.AddComponent<Level>();
        level.Info = info;
        return level;
    }

    private void Start() {
        InitLevelSceneObj( Info );
    }

    /// <summary>
    /// Init level content by config info.
    /// </summary>
    /// <param name="info"></param>
    private void InitLevelSceneObj( LevelInfo info ) {
        for( int i = 0; i < info.WreckageList.Length; i++ ) {
            var item = info.WreckageList[i];
            InitWreckage( transform, item );
        }
        Debugger.Assert( BirthWreckage_ != null, "Not config birth wreckage!" );
        for( int i = 0; i < info.ObstacleList.Length; i++ ) {
            var item = info.ObstacleList[i];
            InitObstacle( transform, item );
        }
    }

    private void DestroyLevelSceneObj( LevelInfo info ) { }

    private void InitWreckage( Transform parent, WreckageInfo info ) {
        string modelPath = info.ModelName;
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.Model, modelPath ) as GameObject;

        GameObject root = new GameObject();
        root.name = string.Format( "wreckage_({0})_{1}", root.GetInstanceID(), info.Type );
        root.transform.SetParent( parent );
        root.transform.localPosition = info.Position;
        root.transform.localEulerAngles = info.Face;

        GameObject instance = Utility.CommonInstantiate( prefab, root.transform );
        if( instance == null ) {
            Debug.LogError( "InitWreckage(), failed！" );
            return;
        }
        Collider srcCollider = instance.GetComponent<Collider>();
        AttachCollider( root, srcCollider );
        Destroy( srcCollider );

        //不能着陆的区域碰撞检测
        Transform blockedTransRoot = instance.transform.FindChild( BLOCKED_ROOT_NAME );
        if( blockedTransRoot ) {
            for( int i = 0; i < blockedTransRoot.childCount; i++ ) {
                Transform trans = blockedTransRoot.GetChild( i );
                trans.gameObject.AddComponent<WreckageBlock>();
            }
        }
        else {
            Debugger.LogErrorFormat( "不能着陆的区域碰撞检测的根节点未找到, {0}", name );
        }

        BaseWreckage wreckage = AddScriptByWreckageType( info.Type, root );
        wreckage.StopPoint = InitStopPoint( root.transform, info );

        if( info.Nodetype == WreckageInfo.NodeType.Birth ) {
            BirthWreckage = wreckage;
        }
        else if( info.Nodetype == WreckageInfo.NodeType.Escape ) {
            wreckage.IsEscape = true;
            LevelGenerator.Instance.EscapeWreckageList.Add( wreckage );
            Debugger.Log( "Is escape: " + root.name );
        }
        else { }

        List<Vector3> sourceGenPos = GetSourcePointList( info.SourcePointList, info.SourceInfoList.Length );
        Debugger.Assert( sourceGenPos.Count == info.SourceInfoList.Length, "资源点位置数量与资源点数量不一致！" );
        List<BaseSource> sourcePosList = new List<BaseSource>();
        for( int i = 0; i < info.SourceInfoList.Length; i++ ) {
            BaseSource source = InitSourcePoints( root.transform, info.SourceInfoList[i], sourceGenPos[i] );
            sourcePosList.Add( source );
        }
        wreckage.SourcePointList = sourcePosList;
    }

    private GameObject InitStopPoint( Transform parent, WreckageInfo info ) {
        GameObject stopPointGo = new GameObject( "__StopPoint__" );
        stopPointGo.transform.SetParent( parent.transform );
        stopPointGo.transform.localPosition = info.StopPoint;
        stopPointGo.transform.localEulerAngles = info.StopPointFace;
        return stopPointGo;
    }

    private BaseWreckage AddScriptByWreckageType( WreckageInfo.WreckageType type, GameObject target ) {
        switch( type ) {
            case WreckageInfo.WreckageType.Normal:
                return target.AddComponent<NormalWreckage>();
            case WreckageInfo.WreckageType.ElectricBaseStation:
                return target.AddComponent<ElectricBaseStation>();
            case WreckageInfo.WreckageType.CoolRoom:
                return target.AddComponent<CoolRoom>();
            case WreckageInfo.WreckageType.LoseElectricZone:
                return target.AddComponent<LoseElectricZone>();
            case WreckageInfo.WreckageType.FuelRoom:
                return target.AddComponent<FuelRoomWreckage>();
            case WreckageInfo.WreckageType.HeatPower:
                return target.AddComponent<HeatPowerWreckage>();
            case WreckageInfo.WreckageType.ElectricCore:
                return target.AddComponent<ElectricCore>();
            default:
                return null;
        }
    }

    private BaseSource AddScriptBySourceType( SourceInfo.SourceType type, GameObject target ) {
        switch( type ) {
            case SourceInfo.SourceType.Oxygen:
                return target.AddComponent<OxygenPoint>();
            case SourceInfo.SourceType.Fuel:
                return target.AddComponent<FuelPoint>();
            case SourceInfo.SourceType.ChestA:
                return target.AddComponent<ChestAPoint>();
            case SourceInfo.SourceType.ChestB:
                return target.AddComponent<ChestBPoint>();
            case SourceInfo.SourceType.ChestC:
                return target.AddComponent<ChestCPoint>();
            default:
                return null;
        }
    }

    private void InitObstacle( Transform parent, ObstacleInfo info ) {
        string modelPath = info.ModelName;
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.Model, modelPath ) as GameObject;
        string name = string.Format( "obstacle_{0}", info.ID );

        GameObject instance = Utility.CommonInstantiate( prefab, parent, info.Position, info.Face );
        if( instance ) {
            instance.name = name;
            //Debug.Assert( instance.transform.childCount == 1, string.Format( "Obstacle must have only one animation node! [{0}]", name ) );
            Transform animNode = instance.transform.childCount == 1 ? instance.transform.GetChild( 0 ) : instance.transform;
            switch( info.Type ) {
                case WreckageInfo.WreckageType.Normal:
                    animNode.gameObject.AddComponent<NormalObstacle>();
                    break;
                case WreckageInfo.WreckageType.ElectricBaseStation:
                    animNode.gameObject.AddComponent<Obstacle>();
                    break;
                case WreckageInfo.WreckageType.CoolRoom:
                    animNode.gameObject.AddComponent<Obstacle>();
                    break;
                case WreckageInfo.WreckageType.LoseElectricZone:
                    instance.AddComponent<LoseElectricObstacle>();
                    break;
                case WreckageInfo.WreckageType.FuelRoom:
                    animNode.gameObject.AddComponent<Obstacle>();
                    break;
                case WreckageInfo.WreckageType.HeatPower:
                    animNode.gameObject.AddComponent<Obstacle>();
                    break;
                default:
                    break;
            }
        }
        else {
            Debugger.LogErrorFormat( "生成障碍失败！障碍名：{0}", info.ModelName );
        }
    }

    private BaseSource InitSourcePoints( Transform parent, SourceInfo info, Vector3 pos ) {
        string modelPath = GetSourceModelPathByType( info.Type );
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.Model, modelPath ) as GameObject;
        string name = string.Format( "source_{0}", info.Type );
        GameObject root = new GameObject( name );
        root.transform.SetParent( parent );
        root.transform.localEulerAngles = Vector3.zero;
        //set local position
        root.transform.localPosition = pos;
        GameObject instance = Utility.CommonInstantiate( prefab, root.transform );
        if( instance == null ) {
            Debug.LogError( "Create source point model failed, instance is null!" );
            return null;
        }
        BoxCollider srcCollider = instance.GetComponent<BoxCollider>();
        if( srcCollider ) {
            AttachCollider( root, srcCollider );
            Destroy( srcCollider );
        }
        else {
            Debugger.LogErrorFormat( "资源点父节点上未找到BoxCollider: {0}", name );
        }

        BaseSource currentSource = AddScriptBySourceType( info.Type, root );
        currentSource.EffectName = info.PickUpEffectName;
        currentSource.Point = info.EnergyAmount;
        RecordSpecialSourcePoints( info, currentSource );
        return currentSource;
    }

    private void RecordSpecialSourcePoints( SourceInfo info, BaseSource targetSourceInfo ) {
        if( info.Type == SourceInfo.SourceType.ChestA
           || info.Type == SourceInfo.SourceType.ChestB
           || info.Type == SourceInfo.SourceType.ChestC ) {
            LevelGenerator.Instance.ChestPointList.Add( targetSourceInfo );
        }
    }

    private string GetSourceModelPathByType( SourceInfo.SourceType type ) {
        string modelPath = string.Empty;
        if( GlobalConfig.ModelToSourceType.ContainsKey( type.ToString() ) ) {
            modelPath = GlobalConfig.ModelToSourceType[type.ToString()];
            //Debug.Log( "GetSourceModelPathByType(), " + modelPath );
        }
        else {
            Debug.LogError( "not have the config of source model: " + type.ToString() );
        }
        return modelPath;
    }

    private void OnTriggerEnter( Collider other ) {
        LevelGenerator.Instance.CurrenEnterLevel = this;
    }

    private void OnTriggerExit( Collider other ) {
        LevelGenerator.Instance.CurrenEnterLevel = null;
    }

    private void AttachCollider( GameObject target, Collider srcCollider ) {
        if( srcCollider == null ) {
            Debug.LogError( "AttachCollider(), failed! srcCollider is null!" );
            return;
        }
        if(srcCollider is BoxCollider ) {
            BoxCollider result = target.GetComponent<BoxCollider>();
            if( result == null ) {
                result = target.AddComponent<BoxCollider>();
            }
            result.isTrigger = true;
            BoxCollider collider = srcCollider as BoxCollider;
            result.center = collider.center;
            result.size = new Vector3( collider.size.x * srcCollider.transform.localScale.x,
                                       collider.size.y * srcCollider.transform.localScale.y,
                                       collider.size.z * srcCollider.transform.localScale.z
                                       );
        }
        else if ( srcCollider is SphereCollider ) {
            SphereCollider result = target.GetComponent<SphereCollider>();
            if( result == null ) {
                result = target.AddComponent<SphereCollider>();
            }
            result.isTrigger = true;
            SphereCollider collider = srcCollider as SphereCollider;
            result.center = collider.center;
            result.radius = collider.radius;
        }
    }


    private List<Vector3> GetSourcePointList( Vector3[] src, int sourcePointCount ) {
        Debugger.Assert( src.Length >= sourcePointCount, "资源点的数量比可随机的资源点的位置数还多！" );

        //要求每一个资源点的位置都是不一样的
        List<int> differentPointList = new List<int>();

        int foundCount = 0;
        while( foundCount < sourcePointCount ) {
            int index = Random.Range( 0, src.Length );
            if( !differentPointList.Contains( index ) ) {
                //Debugger.Log("Found index: " + index);
                differentPointList.Add( index );
                foundCount++;
            }
        }

        //Debugger.LogFormat( "随机的资源点位置列表个数： {0}", differentPointList.Count );
        string msg = "随机的资源点位置列表: ";
        for( int i = 0; i < differentPointList.Count; i++ ) {
            msg += string.Format( "{0}->{1}, ", differentPointList[i], src[differentPointList[i]] );
        }
        //Debugger.Log(msg);

        List<Vector3> points = new List<Vector3>();
        for( int j = 0; j < differentPointList.Count; j++ ) {
            points.Add( src[j] );
        }
        return points;
    }

}
