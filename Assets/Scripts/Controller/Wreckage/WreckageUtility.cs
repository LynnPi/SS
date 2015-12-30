using UnityEngine;
using System.Collections;

public class WreckageUtility : MonoBehaviour{
    private Material BodyMat_;

    private Texture BodyOriginTex_;
    private Texture BodyElectricShockTex_;

    private GameObject Backpack_;
    private GameObject EquipEffect_;
    private string ElectricEffectName_ = "electric_01";
    public bool IsShocking = false;
    private Player Player_;
    private float Timer_;
    private float SwitchTimer_;
    private float SwitchPerSec_;
    private float Duration_;
    private bool IsDecOxygen_;
    private float CurrOxygen_;
    private float DecPerDelta_;
    private bool IsLoseElectric_;
    private GameObject EffectGo_;
    public bool ShouldUpdateOxygen=true;

    private void InitData( float duration, float switchPerSec, bool isDecOxygen = false, bool isLoseElectric = false ) {
        Timer_ = 0.0f;
        SwitchTimer_ = 0.0f;
        IsShocking = true;
        Duration_ = duration;
        SwitchPerSec_ = switchPerSec;
        IsDecOxygen_ = isDecOxygen;
        IsLoseElectric_ = isLoseElectric;
        CurrOxygen_ = PlayerDataCenter.CurrentRoleInfo.Oxygen;
        ShouldUpdateOxygen = !isDecOxygen;

        if( null == Player_ ) {
            Player_ = ExploreController.Instance.CurrentPlayer;
            BodyMat_ = Player_.transform.FindChild( "Body" ).GetComponent<SkinnedMeshRenderer>().material;
            BodyElectricShockTex_ = AssetBundleLoader.Instance.GetAsset( AssetType.BuiltIn, "Login/textures/body" ) as Texture;
            Backpack_ = Player_.transform.FindChild( "Backpack_01" ).gameObject;
            EquipEffect_ = Player_.transform.FindChild( "CS_Root/CS Pelvis/CS Spine/Equip Point" ).gameObject;
            EffectGo_ = GetElectricEffect();
        }
        BodyOriginTex_ = BodyMat_.mainTexture;
        DecPerDelta_ = (isLoseElectric ? ExploreController.Instance.GetDecValWhenLoseElecric() : CurrOxygen_) / Duration_ * Time.fixedDeltaTime;
        //player.CurrentRole.RoleAnimator.SetTrigger( "" );//以后有动画再用
        EffectGo_.SetActive( true );
    }

    public IEnumerator ElectricShock( float duration, float switchPerSec, bool isDecOxygen = false, bool isLoseElectric = false ) {
        UIManager.Instance.PlayUISound( "Sound/electric" );
        InitData( duration, switchPerSec, isDecOxygen, isLoseElectric );
        while( true ) {
            Timer_ += Time.fixedDeltaTime;
            SwitchTimer_ += Time.fixedDeltaTime;
            DecOxygen();
            if( SwitchTimer_ >= SwitchPerSec_ ) {
                ChangeTexture();
                SwitchTimer_ = 0.0f;
            }
            if( Timer_ >= Duration_ ) {
                OnElectricShockFinished();
                break;
            }
            yield return new WaitForFixedUpdate();//受Time.timeScale影响
        }
    }

    //test
    //private float t1, t2;
    //public IEnumerator ElectricShock( float duration, float switchPerSec, bool isDecOxygen = false, bool isLoseElectric = false ) {
    //    UIManager.Instance.PlayUISound( "Sound/electric" );
    //    InitData( duration, switchPerSec, isDecOxygen, isLoseElectric );
    //    t1 = Time.time;
    //    t2 = Time.time;
    //    while( true ) {
    //        DecOxygen();
    //        if( Time.time - t2 >= SwitchPerSec_ ) {
    //            ChangeTexture();
    //            t2 = Time.time;
    //        }
    //        if( Time.time-t1 >= Duration_ ) {
    //            OnElectricShockFinished();
    //            break;
    //        }
    //        yield return new WaitForSeconds(Time.deltaTime);//受Time.timeScale影响
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <returns>是否切换完成</returns>
    private void ChangeTexture() {
        BodyMat_.mainTexture = BodyOriginTex_ == BodyMat_.mainTexture ? BodyElectricShockTex_ : BodyOriginTex_;
        Backpack_.SetActive( BodyOriginTex_ == BodyMat_.mainTexture );
        EquipEffect_.SetActive( Backpack_.activeSelf );
    }

    private void OnElectricShockFinished() {
        if( IsDecOxygen_ ) {
            if( IsLoseElectric_ ) {
                ExploreController.Instance.SetOxygenWhenLoseElectrc(PlayerDataCenter.CurrentRoleInfo.Oxygen-ExploreController.Instance.GetDecValWhenLoseElecric());
                ExploreController.Instance.OnMeetLoseElectric();
            }
            else {
                ExploreController.Instance.SetOxygenWhenLoseElectrc( 0 );
                ExploreController.Instance.Die();
            }
        }
        Timer_ = 0.0f;
        SwitchTimer_ = 0.0f;
        BodyMat_.mainTexture = BodyOriginTex_;
        IsShocking = false;
        ShouldUpdateOxygen = true;
        Backpack_.SetActive( true );
        EquipEffect_.SetActive( true );
        Player_.FSM.SendEvent( "BlockedFinished" );
        if( null != EffectGo_ ) {
            Invoke( "SetEffectDeactive", 0.5f );
        }
    }

    private void SetEffectDeactive() {
        EffectGo_.SetActive( false );
    }

    private void DecOxygen() {
        if( IsDecOxygen_ ) {
            CurrOxygen_ -= DecPerDelta_;
            ExploreController.Instance.SetOxygenWhenLoseElectrc( CurrOxygen_ );
        }
    }

    private GameObject GetElectricEffect() {
        GameObject prefab = AssetBundleLoader.Instance.GetAsset( AssetType.Effect, ElectricEffectName_ ) as GameObject;
        if( prefab ) {
            prefab = Utility.CommonInstantiate( prefab, Player_.transform );
        }
        return prefab;
    }
}
