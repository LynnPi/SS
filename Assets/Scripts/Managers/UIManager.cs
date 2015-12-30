using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    /// <summary>
    /// 已经打开的UI列表
    /// </summary>
    protected List<GameObject> OpenedUIList = new List<GameObject>();

    /// <summary>
    /// 已经关闭的UI列表
    /// </summary>
    protected List<List<GameObject>> ClosedUIList = new List<List<GameObject>>();

    void Awake() {
        Instance = this;
    }

    #region Canvas渲染层级相关

    private Canvas PanelCanvas_;
    /// <summary>
    /// 常规界面层
    /// </summary>
    public Canvas PanelCanvas {
        get {
            if (Instance.PanelCanvas_ == null) {
                Instance.PanelCanvas_ = Instance.transform.FindChild("Canvas-UIPanel").GetComponent<Canvas>();
            }
            return Instance.PanelCanvas_;
        }
    }

    private Canvas SceneUICanvas_;
    /// <summary>
    /// 场景动态UI层，如血条
    /// </summary>
    public Canvas SceneUICanvas {
        get {
            if (Instance.SceneUICanvas_ == null) {
                Instance.SceneUICanvas_ = Instance.transform.FindChild("Canvas-SceneUI").GetComponent<Canvas>();
            }
            return Instance.SceneUICanvas_;
        }
    }

    private Canvas PopWindowCanvas_;
    /// <summary>
    /// 弹窗层
    /// </summary>
    public Canvas PopWindowCanvas {
        get {
            if (Instance.PopWindowCanvas_ == null) {
                Instance.PopWindowCanvas_ = Instance.transform.FindChild("Canvas-PopWindow").GetComponent<Canvas>();
            }
            return Instance.PopWindowCanvas_;
        }
    }

    #endregion

    private UIPanelBehaviour GetScript(GameObject go) {
        return go.GetComponent<UIPanelBehaviour>();
    }

    private IEnumerator ClosePanel(UIPanelBehaviour script, bool bPlaySound, bool bDelay = true) {
        yield return StartCoroutine(script.Close(bPlaySound, bDelay));
    }

    private IEnumerator ClosePanel(GameObject go, bool bPlaySound) {
        yield return StartCoroutine(ClosePanel(GetScript(go), bPlaySound));
    }

    private void ShowUIListContent() {}

    private IEnumerator OnShowPanel(UIPanelBehaviour behaviour, object[] args) {
        string uiName = behaviour.name;
        UIDisplayControl ctrl = GlobalConfig.GetUIDisplayControl(uiName);
        ClearAllPanel(ctrl);
        string dontCloseUI = ctrl != null ? ctrl.DontClose : string.Empty;
        if (dontCloseUI != "All" && !string.IsNullOrEmpty(dontCloseUI)) {
            CloseAllPanel(dontCloseUI.Split(','));
        }
        StartCoroutine(behaviour.Show(args));
        OpenedUIList.Add(behaviour.gameObject);
        ShowUIListContent();
        yield return null;
    }

    private IEnumerator OnClosePanel(UIPanelBehaviour script, bool bPlaySound) {
        bool bActive = script.GetActiveSelf();
        if (!bActive) yield break;
        yield return StartCoroutine(ClosePanel(script, bPlaySound));
        OpenedUIList.RemoveAt(OpenedUIList.Count - 1);
        string uiName = script.name;
        UIDisplayControl ctrl = GlobalConfig.GetUIDisplayControl(uiName);
        if (ctrl != null) {
            if (ctrl.JustCloseSelfWhenClosing)
                yield break;
        }
        if (ClosedUIList.Count <= 0) yield break;
        List<GameObject> lastClosedUIList = ClosedUIList[ClosedUIList.Count - 1];
        for (int i = 0; i < lastClosedUIList.Count; i++) {
            GameObject panel = lastClosedUIList[i];
            if (!panel) continue;
            UIPanelBehaviour panelScript = panel.GetComponent<UIPanelBehaviour>();
            StartCoroutine(panelScript.ReShow());
            OpenedUIList.Add(panel);
        }
        ClosedUIList.RemoveAt(ClosedUIList.Count - 1);
        ShowUIListContent();
    }

    private T GetPanel<T>() where T : UIPanelBehaviour {
        string uiName = typeof(T).Name;   
        Transform parentTrans = PanelCanvas.transform;
        if (uiName.Contains("popwindow") || uiName.Contains("dialog"))
            parentTrans = PopWindowCanvas.transform;
        else if (uiName.Contains("sceneui"))
            parentTrans = SceneUICanvas.transform;
        Transform oldPanel = parentTrans.FindChild(uiName);
        if (!oldPanel) {
            GameObject newPanel = CreateUI(uiName, parentTrans.gameObject);
            if (!newPanel) {
                //Debug.LogError("CreateUI failed: " + uiName);
                return null;
            }
            oldPanel = newPanel.transform;
            newPanel.AddComponent<T>();
        }
        return oldPanel.GetComponent<T>();
    }

    private void ClearAllPanel(UIDisplayControl ctrl) {
        if (ctrl == null) return;
        if (!ctrl.ClearOpened) return;
        for (int i = 0; i < OpenedUIList.Count; i++) {
            GameObject panel = OpenedUIList[i];
            // 不存在的跳过
            if (!panel) continue;
            UIPanelBehaviour script = panel.GetComponent<UIPanelBehaviour>();
            // 已关闭的跳过
            if (!script.GetActiveSelf()) continue;
            // 关闭该界面
            StartCoroutine(ClosePanel(script, false));
        }
        OpenedUIList.Clear();
        ClosedUIList.Clear();
    }

    private void CloseAllPanel(params string[] dontCloseUIName) {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < OpenedUIList.Count; i++) {
            GameObject panel = OpenedUIList[i];
            // 不存在的跳过
            if (!panel) continue;
            UIPanelBehaviour script = panel.GetComponent<UIPanelBehaviour>();
            // 已关闭的跳过
            if (!script.GetActiveSelf()) continue;
            // 过滤
            bool b = false;
            for (int k = 0; k < dontCloseUIName.Length; k++) {
                if (panel.name == dontCloseUIName[k]) {
                    b = true;
                    break;
                }
            }
            if (b) continue;
            // 关闭该界面
            StartCoroutine(ClosePanel(script, false));
            // 加入到关闭表内
            list.Add(OpenedUIList[i]);
        }
        // 把该阶段的界面加入关闭表
        if (list.Count > 0)
            ClosedUIList.Add(list);
        // 把关闭的UI从打开表内清除
        for (int i = 0; i < list.Count; i++) {
            if (OpenedUIList.Contains(list[i]))
                OpenedUIList.Remove(list[i]);
        }
    }

    #region interface
    public static void ClosePanel<T>(bool bPlaySound = true) where T : UIPanelBehaviour {
        if (!Instance) return;
        T go = Instance.GetPanel<T>();
        if (!go) return;
        Instance.StartCoroutine(Instance.OnClosePanel(go, bPlaySound));
    }

    public static void ShowPanel<T>(params object[] args) where T : UIPanelBehaviour {
        if (!Instance) return;
        T go = Instance.GetPanel<T>();
        if (!go) return;
        Instance.StartCoroutine(Instance.OnShowPanel(go, args));
    }
    #endregion

    public GameObject CreateUI(string uiName, GameObject attachParent = null) {
        //Debugger.Log("CreateUI: " + uiName);
        Object go = AssetBundleLoader.Instance.GetAsset(AssetType.UI, uiName);

        if(go == null ) {
            Debugger.LogErrorFormat("创建UI失败，未找到对应UI资源: {0}", uiName );
            return null;
        }

        GameObject ui = Instantiate(go) as GameObject;

        ui.name = uiName;
        if (attachParent) {
            ui.transform.SetParent(attachParent.transform);
        }
        ui.transform.localScale = Vector3.one;
        ui.transform.localPosition = Vector3.zero;
        RectTransform rt = ui.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;//ui面板归位
        rt.sizeDelta = Vector2.zero;//统一以锚点为基准expand

        return ui;
    }

    public GameObject CreateUIEffect(string effectName, GameObject attachParent = null) {
        Object go = AssetBundleLoader.Instance.GetAsset(AssetType.UI, effectName);
        GameObject effect = Instantiate(go) as GameObject;

        if (attachParent) {
            effect.transform.SetParent(attachParent.transform);
        }
        effect.transform.localScale = Vector3.one;
        effect.transform.localPosition = Vector3.zero;

        return effect;
    }

    public void PlayUISound( string soundName ) {
        Object obj = AssetBundleLoader.Instance.GetAsset( AssetType.Audio, soundName );
        AudioClip clip = obj as AudioClip;
        if( obj == null ) {
            return;
        }
        if( clip == null ) {
            return;
        }
        AudioManager.Instance.PlaySound( clip, Camera.main.transform.position, false );
    }
}