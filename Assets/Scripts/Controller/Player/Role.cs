using UnityEngine;
using System.Collections;

/// <summary>
/// Manage role model and animation etc.
/// </summary>
public class Role : MonoBehaviour {

    public static Role Create(string modelName, Transform parent) {//Resources.Load( "Thesurvivors_01" ) as GameObject; 
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.Model, modelName ) as GameObject;
        GameObject go = Utility.CommonInstantiate( prefab, parent, ClientConfig.TAG_PLAYER, "Player" );
        BoxCollider collider = go.GetComponent<BoxCollider>();
        if( collider ) {
            //带刚体的碰撞盒需要置isTrigger为true,来关闭物体碰撞。
            collider.isTrigger = true;
        }
        else {
            Debugger.LogError("角色未添加碰撞盒！");
        }
        return go.AddComponent<Role>();
    }

    public enum AnimState {
        Standard_To_Ready,
        Ready_To_Standard,
        Any_To_Hit
    }

    private Animator RoleAnimator_;
    public Animator RoleAnimator {
        get {
            if( RoleAnimator_ == null ) {
                RoleAnimator_ = GetComponent<Animator>();
                //foreach( var item in RoleAnimator_.parameters ) {
                //    Debugger.Log( "RoleAnimator_.parameter: " + item.name );
                //}
            }
            return RoleAnimator_;
        }
    }

    public void PlayAnimation( AnimState state) {
        if( RoleAnimator == null ) {
            Debugger.LogError("角色Animator组件为空！");
            return;
        }

        switch( state ) {
            case AnimState.Standard_To_Ready:
                RoleAnimator.SetTrigger( "Standard_To_Ready" );
                break;
            case AnimState.Ready_To_Standard:
                RoleAnimator.SetTrigger( "Ready_To_Standard" );
                break;
            case AnimState.Any_To_Hit:
                RoleAnimator.SetTrigger( "Any_To_Hit" );
                break;
            default:
                break;
        }
    }
}
