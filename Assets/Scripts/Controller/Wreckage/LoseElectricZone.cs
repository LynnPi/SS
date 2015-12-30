using UnityEngine;
using System.Collections;

public class LoseElectricZone : BaseWreckage {
    //注意：以下代码等到策划把漏电残骸配出来再移到LoseElectricZone去，这里先作测试用
    private float Force_ = 10;
    private float Duration_ = 10;
    private float SwitchPerSec_ = 0.04f;
    private Player Role_;

    public override void OnTriggerEnter( Collider other ) {
        if( other.tag == ClientConfig.TAG_PLAYER ) {
            if( ExploreController.Instance.WreUtility.IsShocking ) {
                return;
            }
            if( null == Role_ ) {
                Role_ = ExploreController.Instance.CurrentPlayer;
            }
            Role_.FSM.SendEvent( "Blocked" );
            Role_.RigidBody.velocity = Vector3.zero;
            Role_.RigidBody.AddForce( -Role_.transform.forward * Force_, ForceMode.Impulse );
            StartCoroutine( ExploreController.Instance.WreUtility.ElectricShock( Duration_, SwitchPerSec_, true, true ) );
        }
    }
}
