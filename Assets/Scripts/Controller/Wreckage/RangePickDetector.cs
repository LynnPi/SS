using UnityEngine;
using System.Collections;

public class RangePickDetector : MonoBehaviour {
    private void OnTriggerEnter( Collider other ) {

        LevelGenerator.Instance.CurrentWreckage = transform.parent.parent.GetComponent<BaseWreckage>();

        ExploreController.Instance.PickUp.OnPlayerLanded();
    }

    private void OnTriggerExit( Collider other ) {
        ExploreController.Instance.PickUp.OnPlayerAway();
    }
}
