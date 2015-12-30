using UnityEngine;
using System.Collections;

public class NormalObstacle : Obstacle {
    public override void OnTriggerEnter( Collider other ) {
        if( other.tag == ClientConfig.TAG_PLAYER ) {
            base.OnTriggerEnter( other );
            CurrPlayer.OnBlockedOff();
            CurrPlayer.CurrentRole.PlayAnimation( Role.AnimState.Any_To_Hit );
        }
    }
}
