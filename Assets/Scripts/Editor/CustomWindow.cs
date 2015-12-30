using UnityEngine;
using System.Collections;
using UnityEditor;
public class CustomWindow : EditorWindow {

	[MenuItem("CustomMenu/更新工程")]
    public static void UpdateProject() {
        Application.OpenURL("http://internal.palmpioneer.com/ss/git2svn.php");
    }
}
