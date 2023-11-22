
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Compilation;
using System.Runtime.InteropServices.ComTypes;
using System;

public class WXProjectSettingsProvider : SettingsProvider
{
    private GUIStyle buttonStyle;
    enum CodeOptimization
    {
        BuildTimes,
        RuntimeSpeed,
 //       RuntimeSpeedLTO,
        DiskSize,
 //       DiskSizeLTO,
    }
    public WXProjectSettingsProvider():base("Project/WXProject Settings", SettingsScope.Project) { }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        InitGUI();
    }

    private void InitGUI()
    {
        //
    }

    public override void OnTitleBarGUI()
    {
        base.OnTitleBarGUI();
        var rect = GUILayoutUtility.GetLastRect();
        buttonStyle = buttonStyle ?? GUI.skin.GetStyle("IconButton");

        var w = rect.x + rect.width;
        rect.x = w - 57;
        rect.y += 6;
        rect.width = rect.height = 18;
        var content = EditorGUIUtility.IconContent("_Help");
        if(GUI.Button(rect, content, buttonStyle)) {
            Application.OpenURL("https://github.com/wechat-miniprogram/minigame-unity-webgl-transform");
        }
    }

    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);
        var _codeOptimizationStr = EditorUserBuildSettings.GetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.WebGL), "CodeOptimization");
        if (Enum.TryParse<CodeOptimization>(_codeOptimizationStr, out CodeOptimization codeOptimization))
        {
            EditorGUI.BeginChangeCheck();
            var codeOptimization_new = EditorGUILayout.EnumPopup("CodeOptimization", codeOptimization);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.WebGL), "CodeOptimization", codeOptimization_new.ToString());
            }
        }
    }

    static WXProjectSettingsProvider provider;
    [SettingsProvider]
    public static SettingsProvider CreateWXProjectSettingsProvider()
    {
        provider = new WXProjectSettingsProvider();
        return provider;
    }
}
