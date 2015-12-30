using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OxygenPoint : BaseSource {
    protected override void AddPoint() {
        ExploreController.Instance.AddPointWhenPickedOxygen( this );
    }
}
