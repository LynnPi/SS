using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using LitJson;

public class UIDisplayControl {

    public string UIName;
    public bool ClearOpened;
    public string DontClose;
    public bool JustCloseSelfWhenClosing;
}

public static class GlobalConfig {
    public static Dictionary<string, string> ModelToSourceType = new Dictionary<string, string>();
    public static Dictionary<string, string> MusicToScene = new Dictionary<string, string>();

    private static Dictionary<string, UIDisplayControl> UIDisplayControlList = new Dictionary<string, UIDisplayControl>();
    public static UIDisplayControl GetUIDisplayControl(string name) {
        UIDisplayControl ctrl = null;
        UIDisplayControlList.TryGetValue(name, out ctrl);
        return ctrl;
    }

    public static bool LoadXml() {
        try {
            // 读配置文件
            TextAsset textAsset = (TextAsset)Resources.Load("Config/Global", typeof(TextAsset));
            if (textAsset == null) {
                Debug.Log("can not load resource Config/Global.xml");
                return false;
            }
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(textAsset.text);

            XmlNodeList NodeList = XmlDoc.SelectSingleNode("Config").ChildNodes;
            foreach (XmlNode Node in NodeList) {
                if (Node.Name == "SysNotice") {
                    //int ID = AnalyzeXmlToInt(Node, "ID");
                    string Content = AnalyzeXmlToString(Node, "Content");
                    if (Content == "") continue;
                    //if (!SysNoticeList.ContainsKey(ID))
                    //    SysNoticeList.Add(ID, Content);
                }
                else if (Node.Name == "UIDisplayControl") {
                    UIDisplayControl ctrl = new UIDisplayControl();
                    ctrl.UIName = AnalyzeXmlToString(Node, "UIName");
                    ctrl.DontClose = AnalyzeXmlToString(Node, "DontClose");
                    ctrl.ClearOpened = AnalyzeXmlToBool(Node, "ClearOpened");
                    ctrl.JustCloseSelfWhenClosing = AnalyzeXmlToBool(Node, "JustCloseSelfWhenClosing");
                    UIDisplayControlList.Add(ctrl.UIName, ctrl);
                }
            }

            //InitFormationList();
            return true;
        }
        catch (Exception err) {
            Debug.Log(err.Message);
            return false;
        }
    }

    private static int AnalyzeXmlToInt(XmlNode Node, string Key) {
        if (Node.Attributes[Key] != null) {
            string TempValue = Node.Attributes[Key].Value;
            if (TempValue.Length > 0)
                return int.Parse(TempValue);
        }
        return 0;
    }

    private static string AnalyzeXmlToString(XmlNode Node, string Key) {
        if (Node.Attributes[Key] != null)
            return Node.Attributes[Key].Value;
        return "";
    }

    private static bool AnalyzeXmlToBool(XmlNode Node, string Key) {
        if (Node.Attributes[Key] != null) {
            string str = Node.Attributes[Key].Value;
            return bool.Parse(str);
        }
        return false;
    }

    public static void ReadConfig() {
        string configFilePath = "Config/BuiltInConfig";
        Utility.ReadJsonToDictionary( configFilePath, "source",ref ModelToSourceType );
        Utility.ReadJsonToDictionary( configFilePath, "music", ref MusicToScene );
    }
}
