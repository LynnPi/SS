using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class Utility : MonoBehaviour {
    public static GameObject CommonInstantiate(GameObject original, Transform parent, string tag = "", string layer="Default") {
        GameObject go = null;
        if( original == null ) {
            Debug.LogError("Instantiate failed, prefab is null!");
            return null;
        }
        go = Instantiate<GameObject>(original);
        if(parent)
            go.transform.SetParent(parent);
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = Vector3.zero;

        Utility.RepelaceShader( go );

        if(tag != string.Empty ) {
            go.tag = tag;
        }

        Utility.SetLayerRecursively( go, layer );

        return go;
    }

    public static GameObject CommonInstantiate( GameObject original, Transform parent, Vector3 pos, string tag = "" ) {
        GameObject go = CommonInstantiate( original, parent, tag );
        if( go ) {
            go.transform.localPosition = pos;
        }
        return go;
    }


    public static GameObject CommonInstantiate( GameObject original, Transform parent, Vector3 pos, Vector3 rotation, string tag = "" ) {
        GameObject go = CommonInstantiate( original, parent );
        if( go ) {
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = rotation;
        }
        return go;
    }

    public static GameObject CommonInstantiate( GameObject original, Transform parent, Vector3 pos, Vector3 rotation, Vector3 scale, string tag = "" ) {
        GameObject go = CommonInstantiate( original, parent );
        if( go ) {
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = rotation;
            go.transform.localScale = scale;
        }       
        return go;
    }

    /// <summary>
    /// Correct the position of the ui object to the specified 3D position in scene. 
    /// </summary>
    public static void ScenePositionToUIPosition( Camera sceneCamera,
                                                  Camera uiCamera,
                                                  Vector3 posInScene,
                                                  Transform uiTarget ) {
        Vector3 viewportPos = sceneCamera.WorldToViewportPoint( posInScene );//unify the position to viewport point.
        Vector3 worldPos = uiCamera.ViewportToWorldPoint( viewportPos );
        uiTarget.position = worldPos;//set world position.
        Vector3 localPos = uiTarget.localPosition;
        localPos.z = 0f;//ignore z axis offset.
        uiTarget.localPosition = localPos;//correct the local position of the ui target.
    }

    /// <summary>
    /// Correct the position of the ui object to the specified 3D position      in scene. 
    /// </summary>
    public static void ScenePositionToUIPosition( Camera sceneCamera,
                                                  Camera uiCamera,
                                                  Vector3 posInScene,
                                                  Transform uiTarget,
                                                  Vector2 offset ) {
        ScenePositionToUIPosition( sceneCamera, uiCamera, posInScene, uiTarget );
        uiTarget.localPosition += (Vector3)offset;
    }

    private static void RepelaceShader( GameObject go ) {

        // 只有编辑器内才需要 repelace shader
        if( !Application.isEditor )
            return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach( Renderer r in renderers ) {
            foreach( Material mat in r.sharedMaterials ) {
                if( !mat ) {
                    Debug.LogError( "Material is null:", go );
                    continue;
                }
                if( !mat.shader ) continue;
                mat.shader = Shader.Find( mat.shader.name );
            }
        }
    }

    public static void SetLayerRecursively( GameObject obj, string layerName ) {
        SetLayerRecursively( obj, LayerMask.NameToLayer( layerName ) );
    }

    public static void SetLayerRecursively( GameObject obj, int layer ) {
        obj.layer = layer;
        foreach( Transform child in obj.transform ) {
            SetLayerRecursively( child.gameObject, layer );
        }
    }

    public static void ReadJsonToDictionary( string filePath, string objName, ref Dictionary<string, string> recordDict ) {
        TextAsset ta = AssetBundleLoader.Instance.GetAsset( AssetType.BuiltIn, filePath ) as TextAsset;
        JsonData data = JsonMapper.ToObject( ta.text );
        var obj = data[objName];
        foreach( var item in obj ) {
            DictionaryEntry entry = (DictionaryEntry)item;
            recordDict.Add( entry.Key.ToString(), entry.Value.ToString() );
        }
    }
}