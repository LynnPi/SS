﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class GameOverPanel : UIPanelBehaviour {

    protected override void OnAwake() {

        Button btn;
        btn = transform.FindChild( "Button_Return" ).GetComponent<Button>();
        btn.onClick.AddListener(OnClickReturn);

    }

    private void OnClickReturn() {
        SceneManager.Instance.EnterScene(SceneType.Quest);
        ExploreController.Instance.PickUp.DestroyAllIcons();
    }

}
