using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System;
using UnityEngine.Windows;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using Codice.Client.BaseCommands;
using static Unity.VisualScripting.Member;
using System.Security.Policy;
using static PlasticPipe.Server.MonitorStats;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Codice.CM.Common;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System.Text;

public class DuelystPListImporter : EditorWindow
{
    private string outPath;
    private bool isUnit = true;
    private List<string> filterList = new List<string>();
    private string filterStr = string.Empty;

    private string texturePackerPath = "C:/Program Files/CodeAndWeb/TexturePacker/bin/TexturePacker.exe";

    private string tpOutPath;
    struct PListFrame
    {
        public string name;
        public string action;
        public int frame;
        public Rect frameRect;
    }

    [MenuItem("Tools/Duelyst PList Importer")]
    public static void Open()
    {
        GetWindow<DuelystPListImporter>("Duelyst PList Importer");
    }

    private bool showSplit = true;
    private bool showTP = true;

    private void OnGUI()
    {


        showSplit = EditorGUILayout.BeginFoldoutHeaderGroup(showSplit, "拆分图集");

        if (showSplit)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {

                isUnit = GUILayout.Toggle(isUnit, new GUIContent("IsUnit"));
                if (GUILayout.Button("选择文件"))
                {
                    string path = EditorUtility.OpenFilePanel("Select PList", "", "plist");
                    if (path.Length != 0)
                    {
                        Import(path);
                    }
                }
                if (GUILayout.Button("选择文件夹"))
                {
                    if (string.IsNullOrEmpty(outPath))
                    {
                        EditorUtility.DisplayDialog("错误", "导出文件夹不能为空", "确定");
                        return;
                    }
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    if (path.Length != 0)
                    {
                        SearchPlistFiles(path);
                    }
                }
                if (GUILayout.Button("导出路径：" + outPath))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    if (path.Length != 0)
                    {
                        outPath = path;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        showTP = EditorGUILayout.BeginFoldoutHeaderGroup(showSplit, "打包TP图集");

        if (showSplit)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                EditorGUILayout.TextField("attack,breathing,death,hit,idle,run");
                EditorGUI.BeginChangeCheck();
                filterStr = EditorGUILayout.TextField(filterStr);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!string.IsNullOrEmpty(filterStr))
                    {
                        var arr = filterStr.Split(",");
                        if (arr.Length > 0)
                        {
                            filterList = new List<string>(arr);
                        }
                    }
                    else
                    {
                        filterList = new List<string>();
                    }
                }
                if (GUILayout.Button("序列镇导出路径：" + tpOutPath))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    if (path.Length != 0)
                    {
                        tpOutPath = path;
                        Debug.LogError(tpOutPath);
                    }
                }
                if (GUILayout.Button("选择文件夹"))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    if (path.Length != 0)
                    {
                        SearchChildFolder(path);
                    }
                }
                if (GUILayout.Button("单独文件夹"))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    if (path.Length != 0)
                    {
                        SearchChildFolder(path);
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    #region 拆分
    public void SearchPlistFiles(string directoryPath)
    {
        try
        {
            // 查找指定目录下的所有 ".plist" 文件
            string[] plistFiles = Directory.GetFiles(directoryPath, "*.plist");

            int len = plistFiles.Length;
            for (int i = 0; i < len - 1; i++)
            {
                try
                {
                    Import(plistFiles[i]);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError(plistFiles[i]);
                }
                EditorUtility.DisplayProgressBar("拆分图集", string.Format("进度：{0}/{1}", i + 1, len - 1), i / len);
            }
            EditorUtility.ClearProgressBar();
        }
        catch (Exception ex)
        {
            Console.WriteLine("查找文件时发生错误：" + ex.Message);
        }
    }

    private void Import(string path)
    {
        if (string.IsNullOrEmpty(outPath))
        {
            EditorUtility.DisplayDialog("错误", "导出文件夹不能为空", "确定");
            return;
        }


        string pngFilePath = path.Replace(".plist", ".png");

        if (!File.Exists(pngFilePath))
        {
            Debug.LogError(string.Format("不存在：{0}", pngFilePath));
            return;
        }

        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader reader = new StreamReader(stream);
        string content = reader.ReadToEnd();
        //Debug.LogError(content);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        reader.Close();
        stream.Close();
        // 获取根节点
        XmlNode root = xmlDoc.DocumentElement;


        // 获取metadata节点
        XmlNode metadataNode = root.SelectSingleNode("dict/key[text()='metadata']/following-sibling::dict");

        // 从metadata节点中获取需要的数据
        int formatValue = int.Parse(metadataNode.SelectSingleNode("key[text()='format']/following-sibling::integer").InnerText);
        string sizeValue = metadataNode.SelectSingleNode("key[text()='size']/following-sibling::string").InnerText;
        string textureFileNameValue = metadataNode.SelectSingleNode("key[text()='textureFileName']/following-sibling::string").InnerText;

        // 在这里使用获取到的数据进行处理
        //Debug.Log("格式：" + formatValue);
        //Debug.Log("尺寸：" + sizeValue);
        //Debug.Log("纹理文件名：" + textureFileNameValue);

        MatchCollection sizeValueMatches = Regex.Matches(sizeValue, @"\d+");

        Vector2 size = new Vector2(int.Parse(sizeValueMatches[0].Value), int.Parse(sizeValueMatches[1].Value));

        // 获取frames节点
        XmlNode framesNode = root.SelectSingleNode("dict/key[text()='frames']/following-sibling::dict");

        PListFrame pListFrame;

        List<PListFrame> frames = new List<PListFrame>();

        string resName = string.Empty;

        float frameSize = 0;

        // 遍历frames子节点
        foreach (XmlNode frameNode in framesNode.ChildNodes)
        {
            // 获取帧名称和帧属性节点
            if (frameNode.LocalName == "key")
            {
                string frameName = frameNode.InnerText;
                //Debug.Log("帧名称：" + frameName);
                pListFrame = new PListFrame();
                pListFrame.name = frameName;

                resName = textureFileNameValue.Replace(".png", "");

                string _frameName = frameName.Replace(resName, "");
                // 使用正则表达式匹配数字
                Match frameMatch = Regex.Match(_frameName, @"\d+");

                if (frameMatch.Success)
                {
                    string number = frameMatch.Value;
                    pListFrame.frame = int.Parse(number);

                    if (isUnit)
                    {
                        int startIndex = frameName.IndexOf(resName) + resName.Length + 1;
                        int endIndex = frameName.IndexOf("_", startIndex);
                        if (endIndex - startIndex - 1 <= 0)
                        {
                            Debug.LogError("frameName:" + frameName);
                            Debug.LogError("resName:" + resName);
                            return;
                        }
                        pListFrame.action = frameName.Substring(startIndex, endIndex - startIndex);
                    }
                }

                XmlNode frameAttributes = frameNode.NextSibling;

                // 从属性节点中获取需要的数据
                string frameValue = frameAttributes.SelectSingleNode("key[text()='frame']/following-sibling::string").InnerText;
                string offsetValue = frameAttributes.SelectSingleNode("key[text()='offset']/following-sibling::string").InnerText;
                //bool rotatedValue = true;
                // if (frameAttributes.SelectSingleNode("key[text()='rotated']/following-sibling::false") != null)
                // {
                //     rotatedValue = false;
                // }
                //bool rotatedValue = bool.Parse(frameAttributes.SelectSingleNode("key[text()='rotated']/following-sibling::false").InnerText);
                string sourceColorRectValue = frameAttributes.SelectSingleNode("key[text()='sourceColorRect']/following-sibling::string").InnerText;
                string sourceSizeValue = frameAttributes.SelectSingleNode("key[text()='sourceSize']/following-sibling::string").InnerText;

                // 在这里使用获取到的数据进行处理

                //Debug.Log("帧数值：" + frameValue);
                //Debug.Log("偏移值：" + offsetValue);
                //Debug.Log("是否旋转：" + rotatedValue);
                // Debug.Log("源颜色矩形：" + sourceColorRectValue);
                // Debug.Log("源尺寸：" + sourceSizeValue);

                // 使用正则表达式匹配数字
                MatchCollection matches = Regex.Matches(frameValue, @"\d+");

                // 遍历匹配结果并输出数字
                int index = 0;
                foreach (Match match in matches)
                {
                    string number = match.Value;
                    switch (index)
                    {
                        case 0:
                            pListFrame.frameRect.x = int.Parse(number);
                            break;
                        case 1:
                            pListFrame.frameRect.y = int.Parse(number);
                            break;
                        case 2:
                            pListFrame.frameRect.width = int.Parse(number);
                            break;
                        case 3:
                            pListFrame.frameRect.height = int.Parse(number);
                            break;
                    }
                    index++;
                }

                frameSize = pListFrame.frameRect.width;

                pListFrame.frameRect.y = size.y - pListFrame.frameRect.y - pListFrame.frameRect.height;

                frames.Add(pListFrame);
            }
        }

        Texture2D sourceTexture = LoadPNG(pngFilePath, size);


        SplitPNG(sourceTexture, frames, resName);

        CreateInfo(resName, frameSize);
    }

    private void CreateInfo(string resName, float frameSize)
    {
        string content = frameSize.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        WriteBytesToFile(bytes, string.Format("{0}/{1}/{2}", outPath, resName, "info.txt"));
    }

    private Texture2D LoadPNG(string pngFilePath, Vector2 size)
    {
        // 读取PNG文件的字节数据
        byte[] pngData = System.IO.File.ReadAllBytes(pngFilePath);

        // 创建Texture2D对象
        Texture2D texture = new Texture2D(2, 2);

        // 加载PNG文件数据到Texture2D
        texture.LoadImage(pngData);

        return texture;
    }

    private void SplitPNG(Texture2D sourceTexture, List<PListFrame> frames, string resName)
    {
        // 获取源纹理的像素数据
        Color[] pixels = sourceTexture.GetPixels();

        // 获取源纹理的宽度和高度
        int width = sourceTexture.width;

        // 拆分纹理
        foreach (PListFrame frame in frames)
        {
            // 计算矩形在像素数组中的起始索引
            int startX = (int)frame.frameRect.x;
            int startY = (int)frame.frameRect.y;
            int startIndex = startY * width + startX;

            // 计算矩形的宽度和高度
            int rectWidth = (int)frame.frameRect.width;
            int rectHeight = (int)frame.frameRect.height;

            // 创建单独的纹理
            Texture2D texture = new Texture2D(rectWidth, rectHeight);
            Color[] rectPixels = new Color[rectWidth * rectHeight];

            // 拷贝矩形区域的像素数据
            for (int y = 0; y < rectHeight; y++)
            {
                for (int x = 0; x < rectWidth; x++)
                {
                    int pixelIndex = startIndex + y * width + x;
                    rectPixels[y * rectWidth + x] = pixels[pixelIndex];
                }
            }

            // 设置单独纹理的像素数据并应用
            texture.SetPixels(rectPixels);
            texture.Apply();

            // 在游戏中使用单独的纹理
            // ...

            // 保存单独的纹理为PNG文件
            byte[] pngData = texture.EncodeToPNG();
            if (string.IsNullOrEmpty(frame.action))
            {
                WriteBytesToFile(pngData, string.Format("{0}/{1}/{2}", outPath, resName, frame.name));
            }
            else
            {
                WriteBytesToFile(pngData, string.Format("{0}/{1}/{2}/{3}", outPath, resName, frame.action, frame.name));
            }

        }
    }

    public void WriteBytesToFile(byte[] bytes, string filePath)
    {
        try
        {
            // 获取文件夹路径
            string directoryPath = Path.GetDirectoryName(filePath);

            // 如果文件夹不存在，则创建文件夹
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 写入字节数组到文件
            File.WriteAllBytes(filePath, bytes);

            //Debug.LogError("成功写入文件：" + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("写入文件时发生错误：" + ex.Message);
        }
    }
    #endregion


    private void SearchChildFolder(string path)
    {
        try
        {
            // 获取指定路径下的第一级子目录
            string[] subdirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            // 输出子目录路径
            foreach (string subdirectory in subdirectories)
            {
                if(subdirectory.IndexOf("tpsheets") != -1)
                {
                    continue;
                }
                var dir = Path.GetFileName(subdirectory);
                string tpOutPathFile = string.Format("{0}/tpsheets/{1}.png", tpOutPath, dir);
                Debug.LogError(tpOutPathFile);
                //Console.WriteLine("子目录：" + subdirectory);
                CreateTPS(texturePackerPath, subdirectory, tpOutPathFile);
            }




        }
        catch (Exception ex)
        {
            Console.WriteLine("读取目录时发生错误：" + ex.Message);
        }
    }

    /*
     * --|Res
     *   --plist
     * --|Out
     *   --|unit
     *      #file.png file.tpsheet
     *   --|fx
     * --|Raw
     *   --|unit
     *     #folder folder_all.tps 
     *   --|fx
     */


    public void CreateTPS(string texturePackerPath, string inputFolderPath, string outputFolderPath)
    {
        try
        {
            var directory = Path.GetFileName(inputFolderPath);
            string tpsPath = string.Format("{0}/{1}.tps", Path.GetDirectoryName(inputFolderPath), directory);
            // 构建命令行参数
            string commandArguments = "";
            commandArguments += string.Format("--sheet {0} ", outputFolderPath);
            commandArguments += string.Format("--data {0}.tpsheet ", outputFolderPath);
            commandArguments += "--format unity-texture2d ";
            commandArguments += "--size-constraints POT ";
            commandArguments += "--trim-mode Trim ";
            commandArguments += "--algorithm Polygon ";
            commandArguments += "--force-squared ";
            commandArguments += string.Format("--save {0} ", tpsPath);
            //commandArguments += "--force-publish ";
            //commandArguments += "--ignore-files */attack/* */death/* ";
            commandArguments += inputFolderPath;

            // 创建 ProcessStartInfo 对象
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = texturePackerPath;
            processInfo.Arguments = commandArguments;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;

            // 执行命令行
            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            process.WaitForExit();

            // 输出命令行输出结果
            string output = process.StandardOutput.ReadToEnd();
            Debug.LogError("CreateTexturePacker 输出：\n" + output);

            if (!File.Exists(tpsPath))
            {
                Debug.LogError(string.Format("不存在：{0}", tpsPath));
                return;
            }

            FileStream stream = new FileStream(tpsPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            reader.Close();
            stream.Close();

            string infoStr = File.ReadAllText(inputFolderPath + "/info.txt");

            if (!string.IsNullOrEmpty(infoStr))
            {
                float spriteSize = int.Parse(infoStr);
                float yPovit = (spriteSize - 17) / spriteSize;

                content = content.Replace("<point_f>0.5,0.5</point_f>", string.Format("<point_f>0.5,{0}</point_f>", yPovit.ToString("0.0000")));
            }


            var key = "<key>ignoreFileList</key>\r\n        <array/>";
            var startIndex = content.IndexOf("<key>ignoreFileList</key>");

            if (filterList.Count > 0)
            {
                var replaceStr = "<key>ignoreFileList</key>\r\n        <array>{0}\r\n        </array>\r\n";
                var filterItem = "\r\n            <string>*/{0}/*</string>";
                var filterStr = string.Empty;
                for (int i = 0; i < filterList.Count; i++)
                {
                    filterStr += string.Format(filterItem, filterList[i]);
                }
                content = content.Substring(0, startIndex) + string.Format(replaceStr, filterStr) + content.Substring(startIndex + key.Length, content.Length - (startIndex + key.Length));
            }



            File.WriteAllText(tpsPath, content);

            ExecuteTPS(texturePackerPath, inputFolderPath, outputFolderPath, tpsPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("调用 TexturePacker 时发生错误：" + ex.Message);
        }
    }


    public void ExecuteTPS(string texturePackerPath, string inputFolderPath, string outputFolderPath, string tpsPath)
    {
        try
        {
            // 构建命令行参数
            string commandArguments = tpsPath;
            commandArguments += string.Format(" --sheet {0} ", outputFolderPath);
            commandArguments += string.Format("--data {0}.tpsheet ", outputFolderPath);
            commandArguments += inputFolderPath;

            // 创建 ProcessStartInfo 对象
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = texturePackerPath;
            processInfo.Arguments = commandArguments;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;

            // 执行命令行
            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            process.WaitForExit();



            // 输出命令行输出结果
            string output = process.StandardOutput.ReadToEnd();
            Debug.LogError("ExecuteTexturePacker 输出：\n" + output);
        }
        catch (Exception ex)
        {
            Debug.LogError("调用 TexturePacker 时发生错误：" + ex.Message);
        }
    }
}
