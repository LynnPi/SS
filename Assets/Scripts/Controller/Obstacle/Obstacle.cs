using UnityEngine;

public class Obstacle : MonoBehaviour {
    protected Player CurrPlayer;

    public virtual void OnTriggerEnter( Collider other ) {
        if (null == CurrPlayer ) {
            CurrPlayer = ExploreController.Instance.CurrentPlayer;
        }
        CurrPlayer.FSM.SendEvent( "Blocked" );
    }
}
