using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {


    public static void CreateEffect(string effectName) {

    }


    public static void CreateUIEffect(string effectName) {

    }


    public static void ChangeRolePackageParticle(bool bOpenNormal) {
        string path1 = "CS_Root/CS Pelvis/CS Spine/Equip Point/energy_02";
        string path2 = "CS_Root/CS Pelvis/CS Spine/Equip Point/energy_01";

        GameObject go1 = ExploreController.Instance.CurrentPlayer.transform.FindChild( path1 ).gameObject;
        GameObject go2 = ExploreController.Instance.CurrentPlayer.transform.FindChild( path2 ).gameObject;

        go1.SetActive( bOpenNormal );
        go2.SetActive( !bOpenNormal );
    }


}
