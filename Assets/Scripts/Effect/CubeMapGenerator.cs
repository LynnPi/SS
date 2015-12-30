using UnityEngine;
using System.Collections;

public class CubeMapGenerator : MonoBehaviour {

    public static CubeMapGenerator Instance_ = null;

    public static CubeMapGenerator Instance {
        get {
            if (Instance_ == null ) {
                GameObject obj = new GameObject( "CubeMapGenerator" );
                Instance_ = obj.AddComponent<CubeMapGenerator>();
            }

            return Instance_;
        }
    }

    private Cubemap CubeMap_;

    void Awake() {

        CubeMap_ = new Cubemap( 256, TextureFormat.ARGB32, false );
        RenderSettings.customReflection = CubeMap_;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
    }

    // 外部手动调用
    public void UpdateCubeMap() {
        StartCoroutine( RendertCubeTexture() );
    }

    private IEnumerator RendertCubeTexture() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        // 取玩家的位置为摄像机位置
        // 暂时暴力取
        {
            if (SceneManager.Instance.CurrentScene == SceneType.Quest ) {
                transform.position = QuestController.Instance.CurrentPlayer.transform.position;
            }else if (SceneManager.Instance.CurrentScene == SceneType.Explore) {
                transform.position = ExploreController.Instance.CurrentPlayer.transform.position;
            }
        }

        GameObject go = new GameObject( "CubemapCamera" + Random.seed );
        var camera = go.AddComponent<Camera>();
        camera.backgroundColor = Color.black;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.cullingMask = 1 << LayerMask.NameToLayer("Default");
        /*camera.transform.position = RenderFromPosition_.position;
        if( RenderFromPosition_.GetComponent<Renderer>() )
            go.transform.position = RenderFromPosition_.GetComponent<Renderer>().bounds.center;
            */
        camera.transform.position = transform.position;

        camera.transform.rotation = Quaternion.identity;

        camera.RenderToCubemap( CubeMap_ );

        DestroyImmediate( go );
    }
}
