
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Panadoll;

public class SpriteAnimatorUtility
{
    [MenuItem("Assets/Create SpriteAnimation from PNG")]
    static void CreateObjFromSelectedFolder()
    {
        var assets = Selection.assetGUIDs;
        foreach (var asset in assets)
        {
            string selectedPath = AssetDatabase.GUIDToAssetPath(asset);
            string[] pngFiles;

            if (Directory.Exists(selectedPath))
            {
                pngFiles = Directory.GetFiles(selectedPath, "*.png");
                Debug.LogError(selectedPath);
            }
            else
            {
                pngFiles = new string[] { selectedPath };
            }


            foreach (string pngFile in pngFiles)
            {
                string spriteSheetPath = pngFile;
                if (string.IsNullOrEmpty(spriteSheetPath))
                {
                    continue;
                }
                string fileName = Path.GetFileNameWithoutExtension(spriteSheetPath);

                TextureImporter textureImporter = AssetImporter.GetAtPath(spriteSheetPath) as TextureImporter;

                if (textureImporter != null)
                {
                    Dictionary<string, List<Sprite>> keyValuePairs = new Dictionary<string, List<Sprite>>();
                    Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(spriteSheetPath);

                    foreach (Object subAsset in subAssets)
                    {
                        Sprite sprite = subAsset as Sprite;

                        if (sprite != null)
                        {
                            var arr = sprite.name.Split('-');
                            if (arr.Length > 0)
                            {
                                var actionName = arr[0];
                                if (keyValuePairs.TryGetValue(actionName, out var sprites))
                                {
                                    sprites.Add(sprite);
                                }else
                                {
                                    keyValuePairs.Add(actionName, new List<Sprite>());
                                    keyValuePairs.TryGetValue(actionName, out sprites);
                                    sprites.Add(sprite);
                                }
                            }
                        }
                    }

                    SpriteAnimationObject SpriteAnimation = ScriptableObject.CreateInstance<SpriteAnimationObject>();
                    SpriteAnimation.SpriteAnimations = new List<SpriteAnimation>();

                    foreach(string key in keyValuePairs.Keys)
                    {
                        SpriteAnimation sa = new SpriteAnimation();
                        sa.Name = key;
                        sa.FPS = 9;
                        sa.Frames = new List<SpriteAnimationFrame>();
                        if(keyValuePairs.TryGetValue(key, out var sprites))
                        {
                            for (int i = 0; i < sprites.Count; i++)
                            {
                                SpriteAnimationFrame saf = new SpriteAnimationFrame();
                                saf.sprite = sprites[i];
                                saf.action = key;
                                sa.Frames.Add(saf);
                            }
                        }
                        SpriteAnimation.SpriteAnimations.Add(sa);
                    }

                    //Debug.LogError(spriteSheetPath);
                    // 指定 ScriptableObject 的保存路径
                    string assetPath = string.Format("Assets/Raw/animations/{0}.asset", fileName);
                    AssetDatabase.CreateAsset(SpriteAnimation, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}