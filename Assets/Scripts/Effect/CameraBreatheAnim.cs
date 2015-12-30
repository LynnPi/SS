using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class CameraBreatheAnim : MonoBehaviour {

    public static CameraBreatheAnim Instance = null;

    [SerializeField]
    public Vector3[] PathPiontList;
	[SerializeField]
	public float AnimationTime = 2.5f;

    public bool Enable {
        set {
            Instance.enabled = value;
            if( value ) {
                StartCoroutine( FloatPosition() );
            }
            else{
                StopAllCoroutines();
                // 一秒内将摄像机归位
                LeanTween.reset();
                LeanTween.moveLocal( this.gameObject, Vector3.zero, 1 ).setEase( LeanTweenType.easeInOutSine );
            }
        }
        get {
            return Instance.enabled;
        }
    }


    void Awake() {
        Instance = this;
        Instance.enabled = false;
    }

    /*void Start() {
        StartCoroutine(FloatPosition());
    }*/

    private IEnumerator FloatPosition() {

        int pathCount = PathPiontList.Length;
        int pathIndex = 0;
        while( true ) {
			LeanTween.moveLocal( this.gameObject, PathPiontList[pathIndex], AnimationTime ).setEase(LeanTweenType.easeInOutSine);
             pathIndex++;
             if( pathIndex == pathCount )
                  pathIndex = 0;
			yield return new WaitForSeconds(AnimationTime);
        }

    }
}