using UnityEngine;
using System.Collections;

public class ElectricCore : BaseWreckage {
    private float Force_ = 10;
    private float Duration_ = 1.0f;
    private float SwitchPerSec_ = 0.02f;
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
            StartCoroutine( ExploreController.Instance.WreUtility.ElectricShock( Duration_, SwitchPerSec_, true, false ) );
        }
    }
}
