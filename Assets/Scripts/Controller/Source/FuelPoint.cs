using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FuelPoint : BaseSource {
    protected override void AddPoint() {
        ExploreController.Instance.AddPointWhenPickedFuel( this );
    }
}
