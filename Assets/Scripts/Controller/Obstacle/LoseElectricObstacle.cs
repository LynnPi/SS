using UnityEngine;
using System.Collections;

public class LoseElectricObstacle : Obstacle {
    private float Duration_ = 1.3f;
    private float SwitchPerSec_ = 0.02f;
    private float Force_ = 20;
    private float Timer_ = 0.0f;

    public override void OnTriggerEnter( Collider other ) {
        if( other.tag == ClientConfig.TAG_PLAYER ) {
            base.OnTriggerEnter( other );
            if( ExploreController.Instance.WreUtility.IsShocking ) {
                return;
            }
            CurrPlayer.RigidBody.velocity = Vector3.zero;
            CurrPlayer.RigidBody.AddForce( -CurrPlayer.transform.forward * Force_, ForceMode.Impulse);
            StartCoroutine( Decelerate() );
            StartCoroutine( ExploreController.Instance.WreUtility.ElectricShock(Duration_, SwitchPerSec_ ) );
        }
    }

    private IEnumerator Decelerate() {
        Timer_ = 0.0f;
        yield return new WaitForSeconds( Force_ * 0.001f );
        Timer_ += Force_ * 0.001f;
        while(Timer_<= Duration_ && CurrPlayer.RigidBody.velocity.sqrMagnitude>0.1) {
            Timer_ += Time.fixedDeltaTime;
            CurrPlayer.RigidBody.velocity -= CurrPlayer.RigidBody.velocity * 0.1f;
            yield return new WaitForFixedUpdate();
        }
    }
}
