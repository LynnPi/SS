using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Object = UnityEngine.Object;

public enum AssetType {
    UI,
    Level,
    Model,
    Audio,
    Effect,
    BuiltIn
}

public class AssetCache {
    public AssetCache( string assetName, Object asset ) {
        AssetName = assetName;
        Asset = asset;
    }
    public string AssetName;
    public Object Asset;
}

public class AssetBundleLoader : MonoBehaviour {
    private static AssetBundleLoader Instance_;
    public static AssetBundleLoader Instance {
        get {
            if (Instance_ == null) {
                GameObject attach = new GameObject("__AssetBundleLoader__");
                Instance_ = attach.AddComponent<AssetBundleLoader>();
            }
            return Instance_;
        }
    }

    public List<string> LoadErrors = new List<string>();

    private AssetBundleManifest ManifestFile_;

    private Dictionary<AssetType, List<AssetCache>> AssetCacheList_ = new Dictionary<AssetType, List<AssetCache>>();

    public Dictionary<AssetType, List<AssetCache>> AssetCacheList {
        get {
            return AssetCacheList_;
        }
    }

    private List<string> LoadedAssetBundles_ = new List<string>();

    public Action<string> LoadMessageUpdateCallback = delegate { };

    private string BaseFolderName {
        get {
            return "res";
        }
    }

    public bool IsLoadingAsset { get; private set; }

    private void Awake() {
        Debug.Log( "persistentDataPath: " + Application.persistentDataPath);
    }

    private IEnumerator UpgradeManifest() {
        string manifestUrl = GetManifestUrl();
        Debug.LogFormat( "manifest url: <color=white>{0}</color>", manifestUrl );

        using( WWW www = new WWW( manifestUrl ) ) {
            yield return www;
            if( !string.IsNullOrEmpty( www.error ) ) {
                Debug.LogErrorFormat( "Upgrade manifest network error: {0} {1}", www.error, manifestUrl );
                LoadErrors.Add( www.error );
                yield break;
            }

            ManifestFile_ = www.assetBundle.LoadAllAssets()[0] as AssetBundleManifest;
            LoadMessageUpdateCallback( "Upgrade manifest file succeed!");
            if( ManifestFile_ ) {
                Debug.Log( "Upgrade manifest succeed!" );
            }
            else {
                Debug.LogError( "Upgrade manifest failed!" );
            }
        }
    }

    private IEnumerator LoadAllBundles() {
        if( ManifestFile_ == null )
            yield break;
        string[] bundleRelativePaths = ManifestFile_.GetAllAssetBundlesWithVariant();
        for( int i = 0; i < bundleRelativePaths.Length; i++ ) {
            string bundlePath = bundleRelativePaths[i];
            string bundleUrl = GetBundleUrl( bundlePath );
            Hash128 hash = ManifestFile_.GetAssetBundleHash( bundlePath );
            yield return StartCoroutine( LoadBundle( bundleUrl, hash ) );
        }
    }

    private IEnumerator LoadBundle(string url, Hash128 hash) {
        if( LoadedAssetBundles_.Contains( url ) ) {
            yield break;
        }
        AssetBundle bundle = null;  
        using( WWW www = WWW.LoadFromCacheOrDownload( url, hash ) ) {
            yield return www;
            if( !string.IsNullOrEmpty( www.error ) ) {
                Debug.LogErrorFormat( "Load bundle network error：{0}, url: {1}", www.error, url );
                LoadErrors.Add( www.error );
                yield break;
            }
            bundle = www.assetBundle;
            string bundleName = url.Substring( url.LastIndexOf( "/" ) + 1, url.Length - url.LastIndexOf( "/" ) -1 );
            LoadMessageUpdateCallback( string.Format( "Download bundle succeed: {0}", bundleName ) );
        } 

        if( bundle == null ) {
            yield break;
        }
        LoadedAssetBundles_.Add( url );
        LoadAllAssets( url, bundle );
    }

    private void LoadAllAssets(string bundleUrl, AssetBundle bundle) {
        foreach( var item in Enum.GetValues( typeof( AssetType ) ) ) {
            AssetType assetType = (AssetType)item;
            string variant = assetType.ToString().ToLower();
            if( bundleUrl.EndsWith( variant ) ) {
                if( !AssetCacheList_.ContainsKey( assetType ) ) {
                    AssetCacheList_.Add( assetType, new List<AssetCache>() );
                }
                string[] bundleNames = bundle.GetAllAssetNames();
                for( int i = 0; i < bundleNames.Length; i++ ) {
                    LoadAsset( bundleNames[i], bundle, assetType );
                }
            }
        }
    }

    private void LoadAsset(string assetName, AssetBundle bundle, AssetType assetType ) {
        Object asset = bundle.LoadAsset( assetName );
        AssetCacheList_[assetType].Add( new AssetCache( asset.name, asset ) );
        //Debug.LogFormat( "record asset: <color=white>{0}</color>", assetName );
        LoadMessageUpdateCallback( string.Format( "Load asset succeed: {0}", asset.name ) );
    }

    private string GetBasePathFromLocal() {
        if( Application.isEditor )
            return "file://" + Application.streamingAssetsPath;
        else if( Application.isMobilePlatform || Application.isConsolePlatform ) {
            if(Application.platform == RuntimePlatform.Android ) {
                return Application.streamingAssetsPath;//For android
            }
            return "file://" + Application.streamingAssetsPath;
        }            
        else //For standalone player. 
            return "file://" + Application.streamingAssetsPath; 
    }

    private string GetBasePathFromRemote() {
#if UNITY_IOS
        return "http://internal.palmpioneer.com/ss/ssgameres/trunk/ios/";
#else        
        return "http://internal.palmpioneer.com/ss/ssgameres/trunk/android/";
#endif
    }

    private string GetBasePath() {
#if IS_LAN
        return GetBasePathFromLocal();
#else
        return GetBasePathFromRemote();
#endif
    }

    private string GetManifestUrl() {
        //manifest file has the same name as base folder in common.
        string mainifestFileName = BaseFolderName;
        string relativePath = Path.Combine( BaseFolderName, mainifestFileName );
       
        return Path.Combine( GetBasePath(), relativePath ).Replace("\\", "/");
    }

    private string GetBundleUrl(string bundlePath) {
        string relativePath = Path.Combine( BaseFolderName, bundlePath );
        return Path.Combine( GetBasePath(), relativePath ).Replace("\\", "/");
    }

    #region public interface
    public IEnumerator Preload() {
        IsLoadingAsset = true;

        //First of all, upgrade manifest which contains all the information of the loading bundles.
        yield return StartCoroutine( UpgradeManifest() );

        //Then, load the bundles and record their asset references.
        yield return StartCoroutine( LoadAllBundles() );

        #region For debug the loaded assets info
        foreach( var k in AssetCacheList_.Keys ) {
            string loadedAssetsInfo = "successfully loaded: ";
            foreach( var v in AssetCacheList_[k] ) {
                loadedAssetsInfo += string.Format( " <color=white>[{0}]</color>", v.AssetName );
            }
            Debug.Log( loadedAssetsInfo );
        }
        #endregion

        if( LoadErrors.Count > 0 )
            LoadMessageUpdateCallback( "Sorry to say loading game resource failed,\n check out please!" );
        else {
            LoadMessageUpdateCallback( "Download game resource completed!" );
            yield return new WaitForSeconds( 0.2f );
        }
           
        Resources.UnloadUnusedAssets();

        IsLoadingAsset = false;
    }

    public Object GetAsset( AssetType type, string assetName ) {
        if( string.IsNullOrEmpty( assetName ) ) {
            Debug.LogError( "GetAsset(), assetName is empty!" );
            return null;
        }

        Object asset = null;
        if( AssetCacheList_.ContainsKey( type ) ) {
            for( int i = 0; i < AssetCacheList_[type].Count; i++ ) {
                AssetCache cache = AssetCacheList_[type][i];
                if( cache.AssetName == assetName )
                    asset = cache.Asset;
            }
        }

        if( asset == null ) {
            //try load from local Resources folder         
            asset = Resources.Load( assetName );
            //Debug.LogFormat( "Resources.Load：{0}， {1}!", assetName, asset ? "succeed" : "failed" );
        }

        if( asset == null ) {
            Debug.LogErrorFormat( "Not found asset: {0}, AssetType is：{1}", assetName, type.ToString() );
        }
        return asset;
    }
    #endregion
}

