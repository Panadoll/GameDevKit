using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Text;
using UnityEditor.SceneManagement;
using System;

namespace Panadoll
{
    [InitializeOnLoad]
    public class UISelector
    {
        private static Dictionary<string, RectTransform> rtDic = new Dictionary<string, RectTransform>();
        private static StringBuilder sb = new StringBuilder();
        private readonly static string PREFS_KEY = "editor_setting_uiselector";
        private static bool enable = true;
        private static bool init = false;

        static UISelector()
        {
            if (init == false)
            {
                init = true;
                enable = PlayerPrefs.GetInt(PREFS_KEY, 0) == 1;
            }
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static RectTransform[] GetAllRectTransforms()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                return prefabStage.prefabContentsRoot.GetComponentsInChildren<RectTransform>();
            }
            else
            {
                return GameObject.FindObjectsOfType<RectTransform>();
            }
        }

        private static bool GetComponents<T>(RectTransform rt)
        {
            var components = rt.GetComponents<T>();
            if (components != null && components.Length > 0)
            {
                sb.Length = 0;
                sb.AppendFormat("{0}.[{1},{2}", rtDic.Count, typeof(T).Name, rt.name);
                rtDic.Add(sb.ToString(), rt);
                return true;
            }
            return false;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var ec = Event.current;
            if (ec != null && ec.button == 1 && ec.type == EventType.MouseUp && enable)
            {
                ec.Use();
                
                Vector2 mousePos = Event.current.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = sceneView.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;

                var rts = GetAllRectTransforms();
                if (rts == null)
                    return;

                Array.Reverse(rts);
                rtDic.Clear();
                foreach (var rt in rts)
                {
                    if (rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, sceneView.camera))
                    {
                        if (GetComponents<Button>(rt)) continue;
                        if (GetComponents<ScrollRect>(rt)) continue;
                        if (GetComponents<Image>(rt)) continue;
                        if (GetComponents<Text>(rt)) continue;
                        if (GetComponents<RawImage>(rt)) continue;
                        //if (GetComponents<RawImage>(rt)) continue;
                    }
                }
                if (rtDic.Count > 0)
                {
                    var menu = new GenericMenu();
                    foreach (var item in rtDic)
                    {
                        menu.AddItem(new GUIContent(item.Key), false, () =>
                        {
                            Selection.activeTransform = item.Value;
                            EditorGUIUtility.PingObject(item.Value.gameObject);
                        });
                    }
                    menu.ShowAsContext();
                }
            }

            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, sceneView.camera.pixelHeight - 30, 100, 30));
            EditorGUI.BeginChangeCheck();
            enable = GUILayout.Toggle(enable, "UISelector");
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetInt(PREFS_KEY, enable ? 1 : 0);
            }
            GUILayout.EndArea();
            Handles.EndGUI();

            
        }
    }
}

