using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class SyncResToLocal : MonoBehaviour {

	[MenuItem("Assets/同步资源到本地")]
    public static void SyncResource() {
        string path = string.Empty;//AssetManager.RemoteResourceUrl;
        Debug.Log( "本地资源文件夹： " + path );
        if( path.Contains( "http" ) ) {
            Debug.LogError("地址错误！");
            return;
        }

        if( Directory.Exists( path ) ) {
            Directory.Delete( path );        
        }
     
        Directory.CreateDirectory( path );

        string targetDir = string.Empty;
        if(Application.platform == RuntimePlatform.IPhonePlayer ) {
            targetDir = "D:/Develop/workspace/spacesurivior/GameRes/trunk/ios/res";
        }
        else {
            targetDir = "D:/Develop/workspace/spacesurivior/GameRes/trunk/android/res";
        }

        if( Directory.Exists( targetDir ) ) {
            Debug.Log( "targetDir： " + targetDir );
            Debug.Log( "path： " + path );
            CopyFolder( targetDir, path );
        }
        else {
            Debug.Log("目标资源文件夹不存在： " + targetDir);
        }

        AssetDatabase.Refresh();
    }

    public static void CopyFolder( string strFromPath, string strToPath ) {
        //如果源文件夹不存在，则创建
        if( !Directory.Exists( strFromPath ) ) {
            Directory.CreateDirectory( strFromPath );
        }
        //取得要拷贝的文件夹名
        string strFolderName = strFromPath.Substring( strFromPath.LastIndexOf( "\\" ) +
          1, strFromPath.Length - strFromPath.LastIndexOf( "\\" ) - 1 );
        //如果目标文件夹中没有源文件夹则在目标文件夹中创建源文件夹
        if( !Directory.Exists( strToPath + "\\" + strFolderName ) ) {
            Directory.CreateDirectory( strToPath + "\\" + strFolderName );
        }
        //创建数组保存源文件夹下的文件名
        string[] strFiles = Directory.GetFiles( strFromPath );
        //循环拷贝文件
        for( int i = 0; i < strFiles.Length; i++ ) {
            //取得拷贝的文件名，只取文件名，地址截掉。
            string strFileName = strFiles[i].Substring( strFiles[i].LastIndexOf( "\\" ) + 1, strFiles[i].Length - strFiles[i].LastIndexOf( "\\" ) - 1 );
            //开始拷贝文件,true表示覆盖同名文件
            File.Copy( strFiles[i], strToPath + "\\" + strFolderName + "\\" + strFileName, true );
        }
        //创建DirectoryInfo实例
        DirectoryInfo dirInfo = new DirectoryInfo( strFromPath );
        //取得源文件夹下的所有子文件夹名称
        DirectoryInfo[] ZiPath = dirInfo.GetDirectories();
        for( int j = 0; j < ZiPath.Length; j++ ) {
            //获取所有子文件夹名
            string strZiPath = strFromPath + "\\" + ZiPath[j].ToString();
            //把得到的子文件夹当成新的源文件夹，从头开始新一轮的拷贝
            CopyFolder( strZiPath, strToPath + "\\" + strFolderName );
        }
    }
}
