using UnityEngine;
using UnityEditor;
using SpaceSurivior;

/// <summary>
/// Demo阶段模拟编辑器
/// </summary>
public class SimulateLevelEditor : EditorWindow {

    [MenuItem( "Assets/Create Level Instance" )]
    public static void CreateLevel() {
        CreateAsset<LevelInfo>("Assets/Resources/Defines/Levels");
    }

    //[MenuItem( "Assets/Create Wreckage Instance" )]
    //public static void CreateWreckage() {
    //    CreateAsset<WreckageInfo>( "Assets/Resources/Defines/Wreckages" );
    //}

    //[MenuItem( "Assets/Create Source Instance" )]
    //public static void CreateSource() {
    //    CreateAsset<SourceInfo>( "Assets/Resources/Defines/SourcePoints" );
    //}

    private static void CreateAsset<T>(string path) {
        string typeName = typeof( T ).Name;
        var instance = ScriptableObject.CreateInstance( typeName );
        string extension = ".asset";
        string fullName = string.Format( "unnamed_{0}{1}", typeName, extension );
        string uniquePath = System.IO.Path.Combine( path, fullName );
        Debug.LogFormat( "Create {0} at path : {1}", typeName, uniquePath );
        AssetDatabase.CreateAsset( instance, uniquePath );
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}