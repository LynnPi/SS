using UnityEngine;
using System.Collections.Generic;
using System;

public class PickUpHelper : MonoBehaviour {
    private int MaxArms_ = 2;
    private List<BaseSource> ListAllIcons_ = new List<BaseSource>();
    private Dictionary<BaseSource, GameObject> DicAllProBarsGo_ = new Dictionary<BaseSource, GameObject>();
    private List<BaseSource> ListPickingIcons_ = new List<BaseSource>();
    private List<BaseSource> ListCurrentWreckage_;
    private Vector3 ViewPortPos_;
    private Vector3 WorldPos_;
    private Vector3 LocalPos_;

    void Update() {
        PickResources();
        AdjustIconsPos();
    }

    public void OnDestroy() {
        DestroyAllIcons();
    }

    #region Public Func
    public void OnPlayerLanded() {
        ListCurrentWreckage_ = LevelGenerator.Instance.CurrentWreckage.SourcePointList;
        BaseSource pick = null;
        for( int i = 0; i < ListCurrentWreckage_.Count; i++ ) {
            pick = ListCurrentWreckage_[i];
            if( false == pick.IsPicked && false == ListAllIcons_.Contains( pick ) ) {
                DicAllProBarsGo_.Add( pick, pick.Init() );
                pick.Set( 100 );
                ListAllIcons_.Add( pick );
            }
        }
        OpenAllIcons();
    }

    public void OnPlayerAway() {
        DestroyAllIcons();
    }

    public void OnOxygenUseOut() {
        OnPlayerAway();
    }

    public void BeginPickup( BaseSource pick ) {
        ListPickingIcons_.Add( pick );
        if( ListPickingIcons_.Count + 1 > MaxArms_ ) {
            CloseExceptPickingIcons();
        }
    }

    public void PickUpComplete( BaseSource pick ) {
        ListPickingIcons_.Remove( pick );
        ListAllIcons_.Remove( pick );
        DicAllProBarsGo_.Remove( pick );
        if( HaveCanBePickedRes() ) {
            OpenExceptPickingIcons();
        }
    }

    public void DestroyAllIcons() {
        for( int i = 0; i < ListAllIcons_.Count; i++ ) {
            ListAllIcons_[i].IsPicking = false;
            ListAllIcons_[i].IsOpen = false;
            ListAllIcons_[i].DestroyProBar();
        }
        ListAllIcons_.Clear();
        ListPickingIcons_.Clear();
        DicAllProBarsGo_.Clear();
    }
    #endregion

    #region Private Func
    private void AdjustIconsPos( GameObject slider, Transform resTran ) {
        ViewPortPos_ = CameraManager.Instance.MainCamera.WorldToViewportPoint( resTran.position );
        WorldPos_ = CameraManager.Instance.UICamera.ViewportToWorldPoint( ViewPortPos_ );
        slider.transform.position = WorldPos_;
        LocalPos_ = slider.transform.localPosition;
        LocalPos_.z = 0f;
        slider.transform.localPosition = LocalPos_;
    }

    private void AdjustIconsPos() {
        for( int i = 0; i < ListAllIcons_.Count; i++ ) {
            if( ListAllIcons_[i].IsOpen ) {
                AdjustIconsPos( DicAllProBarsGo_[ListAllIcons_[i]], ListAllIcons_[i].transform );
            }
        }
    }

    private void PickResources() {
        if( ListAllIcons_.Count == 0 ) return;
        for( int i = 0; i < ListPickingIcons_.Count; i++ ) {
            ListPickingIcons_[i].Picking();
        }
    }

    private void OpenAllIcons() {
        for( int i = 0; i < ListAllIcons_.Count; i++ ) {
            if( false == ListAllIcons_[i].IsOpen ) {
                ListAllIcons_[i].OpenProBar();
            }
        }
    }

    private void OpenExceptPickingIcons() {
        for( int i = 0; i < ListAllIcons_.Count; i++ ) {
            if( false == ListAllIcons_[i].IsPicking ) {
                ListAllIcons_[i].OpenProBar();
            }
        }
    }

    private void CloseExceptPickingIcons() {
        for( int i = 0; i < ListAllIcons_.Count; i++ ) {
            if( false == ListAllIcons_[i].IsPicking ) {
                ListAllIcons_[i].CloseProBar();
            }
        }
    }

    private bool HaveCanBePickedRes() {
        return ListAllIcons_.Count != 0;
    }
    #endregion

}
