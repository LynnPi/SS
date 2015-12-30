using UnityEngine;

/// <summary>
/// The state in quest scene.
/// </summary>
public class ShowInQuestState : FiniteState {
    public override void OnUpdate( object context ) {
        Player owner = context as Player;
        owner.CurrentRole.transform.localEulerAngles = Vector3.up * (30f);
    }
}