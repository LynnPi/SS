using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExploreGuideHelper : MonoBehaviour {
    public enum BubbleDirection {
        FromTopToBottom = -1,
        FromBottomToUp = 1
    }

    private List<SpecialPointGuider> EscapePointGuiderList_ = new List<SpecialPointGuider>();
    private List<SpecialPointGuider> ChestPointGuiderList_ = new List<SpecialPointGuider>();
    private float GuiderOffsetHeight_ = 135f;
    private float TopBoundaryInY_ = 450f;
    private float BottomBoundaryInY_ = -360f;

    private void Awake() {
        InitEscapePointGuiders();
        InitChestPointGuiders();
    }

    private void InitEscapePointGuiders() {
        //TODO: refactor as object pool.
        for( int i = 0; i < LevelGenerator.Instance.EscapeWreckageList.Count; i++ ) {
            BaseWreckage wreckage = LevelGenerator.Instance.EscapeWreckageList[i];
            Transform guideOrigin = CameraManager.Instance.FollowController.transform;
            Transform guideTarget = wreckage.StopPoint.transform; ;
            SpecialPointGuider guider = SpecialPointGuider.Create
                (this, "__EscapePointGuider__", guideTarget, guideOrigin, transform );
            guider.Config( "PositionOutsideGuider", "PositionInsideGuider" );
            guider.Active = true;
            wreckage.Guider = guider;
            EscapePointGuiderList_.Add( guider );
        }
    }

    private void InitChestPointGuiders() {
        for( int i = 0; i < LevelGenerator.Instance.ChestPointList.Count; i++ ) {
            BaseSource source = LevelGenerator.Instance.ChestPointList[i];
            Transform guideOrigin = CameraManager.Instance.FollowController.transform;
            Transform guideTarget = source.transform;
            SpecialPointGuider guider = SpecialPointGuider.Create
                (this, "__ChestPointGuider__", guideTarget, guideOrigin, transform );
            guider.Config( "ResourceOutsideGuider", "ResourceInsideGuider" );
            guider.Active = true;
 
            source.Guider = guider;
            ChestPointGuiderList_.Add( guider );
        }
    }

    private void SortGuiderByOrderOfPosY(ref List<SpecialPointGuider> sortList ) {
        //Bubble sort.
        SpecialPointGuider temp = null;
        for( int i = 0; i < sortList.Count; i++ ) {
            for( int j = i + 1; j < sortList.Count; j++ ) {
                float currPosY = sortList[i].OutsideUI.anchoredPosition.y;
                float nextPosY = sortList[j].OutsideUI.anchoredPosition.y;
                if( nextPosY > currPosY ) {
                    temp = sortList[j];
                    sortList[j] = sortList[i];
                    sortList[i] = temp;
                }
            }
        }
    }

    private BubbleDirection GetCurrentBubbleDirection( List<SpecialPointGuider> sortList ) {
        BubbleDirection direction = BubbleDirection.FromTopToBottom;
        float firstElementPosY = sortList[0].OutsideUI.anchoredPosition.y;
        float lastElementPosY = sortList[sortList.Count - 1].OutsideUI.anchoredPosition.y;
        if( firstElementPosY >= TopBoundaryInY_ ) {
            direction = BubbleDirection.FromTopToBottom;
        }
        if( lastElementPosY <= BottomBoundaryInY_ ) {
            direction = BubbleDirection.FromBottomToUp;
        }
        return direction;
    }

    private void ChangeNamesOfOutsideUIList( List<SpecialPointGuider> sortList ) {
        string leftOrRight = sortList[0].OutsideUI.anchoredPosition.x < 0 ? "Left" : "Right";
        for( int i = 0; i < sortList.Count; i++ ) {
            sortList[i].UIPairRoot.name = string.Format( "OutSideGuider_{0}_{1}", leftOrRight, i );
        }
    }

    private void BubbleUI( List<SpecialPointGuider> sortList, BubbleDirection direction ) {
        if(direction == BubbleDirection.FromBottomToUp ) {
            sortList.Reverse();//reverse the sort list when bubble from the last element to the first.
        }

        for( int i = 0; i < sortList.Count; i++ ) {
            RectTransform targetGuider = sortList[i].OutsideUI;
            if( direction == BubbleDirection.FromTopToBottom ) {
                bool isFirstElement = i == 0;
                if( isFirstElement && targetGuider.anchoredPosition.y > TopBoundaryInY_ ) {
                    float newX = targetGuider.anchoredPosition.x;//not need corrected.
                    targetGuider.anchoredPosition = new Vector2( newX, TopBoundaryInY_ );
                }
            }
            else {
                bool isLastElement = i == sortList.Count - 1;
                if( isLastElement && targetGuider.anchoredPosition.y < BottomBoundaryInY_ ) {
                    float newX = targetGuider.anchoredPosition.x;
                    targetGuider.anchoredPosition = new Vector2( newX, BottomBoundaryInY_ );
                }
            }
           
            for( int j = i + 1; j < sortList.Count; j++ ) {
                RectTransform compareGuider = sortList[j].OutsideUI;          
                Vector3 offset = Vector3.up * GuiderOffsetHeight_ * (int)direction;
                //compared with the previous element and get the offset value by direction.
                compareGuider.anchoredPosition = targetGuider.localPosition + offset;
            }
        }
    }

    private void CorrectAnchorPositionByOrder(List<SpecialPointGuider> sortList) {
        if( sortList.Count > 0 ) {
            SortGuiderByOrderOfPosY( ref sortList );
            ChangeNamesOfOutsideUIList( sortList );
            BubbleDirection bubbleDir = GetCurrentBubbleDirection( sortList );
            BubbleUI( sortList, bubbleDir );
        }                
    }
      
    public void SortOutsideGuiderOnVertical() {
        List<SpecialPointGuider> combinedList = new List<SpecialPointGuider>();
        combinedList.AddRange( EscapePointGuiderList_ );
        combinedList.AddRange( ChestPointGuiderList_ );

        List<SpecialPointGuider> leftOutSideGuiders = new List<SpecialPointGuider>();
        List<SpecialPointGuider> rightOutSideGuiders = new List<SpecialPointGuider>();

        for( int i = 0; i < combinedList.Count; i++ ) {
            SpecialPointGuider guider = combinedList[i];
            if( guider.CurrentState == SpecialPointGuider.State.Inside )
                continue;//Only select outside guiders.
            if(guider.OutsideUI.anchoredPosition.x < 0 ) {
                leftOutSideGuiders.Add( guider );
            }
            else {
                rightOutSideGuiders.Add( guider );
            }
        }

        CorrectAnchorPositionByOrder( leftOutSideGuiders );
        CorrectAnchorPositionByOrder( rightOutSideGuiders );
    }
}

