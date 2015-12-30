using UnityEngine;

public class WreckageBlock : MonoBehaviour {

    public virtual void OnTriggerEnter( Collider other ) {
        Debugger.LogFormat( "WreckageBlock collide with name: {0}, tag: {1}", other.name, other.tag );
        if( other.tag == ClientConfig.TAG_PLAYER ) {
            Player player = ExploreController.Instance.CurrentPlayer;
            if (player.FSM.CurrentStateName != typeof(PrepareLandingState).Name && player.FSM.CurrentStateName != typeof( LandedState ).Name ) {
                player.OnBlockedOff();
                player.CurrentRole.PlayAnimation( Role.AnimState.Any_To_Hit );
                player.FSM.SendEvent( "Blocked" );
            }
        }
    }
}