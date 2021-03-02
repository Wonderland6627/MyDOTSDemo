using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

public class ModelTools
{
    #region 模型动画复制

    public static bool changeName = false;

    [MenuItem("Wonderland6627/ModelTools/开启复制改名")]
    public static void OpenChangeName()
    {
        changeName = true;
    }

    [MenuItem("Wonderland6627/ModelTools/关闭复制改名")]
    public static void CloseChangeName()
    {
        changeName = false;
    }

    [MenuItem("Wonderland6627/ModelTools/选择一个fbx，复制它的动画片段出来")]
    public static void DuplicateModel_sAnimationClip()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("No Selection");
            return;
        }

        string goName = go.name;
        string goPath = AssetDatabase.GetAssetPath(go);//fbx文件路径
        if (!goPath.EndsWith(".fbx") && !goPath.EndsWith("FBX"))
        {
            Debug.LogError("Not fbx");
            return;
        }

        string[] dirs = goPath.Split('/');//folder拆分
        string folderPath = string.Empty;//fbx上一层的文件夹路径
        for (int i = 0; i < dirs.Length - 1; i++)
        {
            folderPath += dirs[i] + "/";
        }

        AnimationClip fbxAnimClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(goPath);
        if (fbxAnimClip == null)
        {
            Debug.LogError("No animclip");
            return;
        }

        DuplicateAnimationClip(fbxAnimClip, folderPath, goName, changeName);
    }

    [MenuItem("Assets/Wonderland6627/ModelTools/选择一堆fbx，复制它们的动画片段出来")]
    [MenuItem("Wonderland6627/ModelTools/选择一堆fbx，复制它们的动画片段出来")]
    public static void DuplicateManyModels_sAnimationClips()
    {
        GameObject[] selectGameObjects = Selection.gameObjects;//文件夹通过GUID来获取路径，原理是文件夹也会有.meta文件

        if (selectGameObjects.Length == 0)
        {
            Debug.LogError("No Folder");
            return;
        }

        List<string> objectsPathList = new List<string>();
        for (int i = 0; i < selectGameObjects.Length; i++)
        {
            objectsPathList.Add(AssetDatabase.GetAssetPath(selectGameObjects[i]));
            Debug.Log(AssetDatabase.GetAssetPath(selectGameObjects[i]));
        }
    }

    [MenuItem("Wonderland6627/ModelTools/选择一个fbx，复制它所有的动画片段出来")]
    public static void DuplicateModel_sAnimationClips()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("No Selection");
            return;
        }

        string goName = go.name;
        string goPath = AssetDatabase.GetAssetPath(go);//fbx文件路径
        if (!goPath.EndsWith("fbx") && !goPath.EndsWith("FBX"))
        {
            Debug.LogError("Not fbx");
            return;
        }

        string[] dirs = goPath.Split('/');//folder拆分
        string folderPath = string.Empty;//fbx上一层的文件夹路径
        for (int i = 0; i < dirs.Length - 1; i++)
        {
            folderPath += dirs[i] + "/";
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(goPath);
        List<AnimationClip> fbxAnimClipList = new List<AnimationClip>();
        for (int i = 0; i < assets.Length; i++)
        {
            Debug.Log(assets[i].GetType());
            if (assets[i].GetType() == typeof(AnimationClip))
            {
                AnimationClip clip = assets[i] as AnimationClip;
                fbxAnimClipList.Add(clip);
            }
        }

        Debug.Log(fbxAnimClipList.Count);
        for (int i = 0; i < fbxAnimClipList.Count; i++)
        {
            DuplicateAnimationClip(fbxAnimClipList[i], folderPath, goName);
        }
    }

    [MenuItem("Assets/Wonderland6627/ModelTools/选择一个文件夹，复制它下面所有fbx的动画片段出来")]
    public static void DuplicateModels_sAnimationClips()
    {
        string[] folderGUIDs = Selection.assetGUIDs;//文件夹通过GUID来获取路径，原理是文件夹也会有.meta文件

        if (folderGUIDs.Length == 0)
        {
            Debug.LogError("No Folder");
            return;
        }

        string folderPath = AssetDatabase.GUIDToAssetPath(folderGUIDs[0]);
        string[] filesPaths = Directory.GetFiles(folderPath);
        List<string> fbxFilesPaths = new List<string>();//选中的所有fbx列表
        for (int i = 0; i < filesPaths.Length; i++)
        {
            if (filesPaths[i].EndsWith(".fbx") || filesPaths[i].EndsWith(".FBX"))
            {
                fbxFilesPaths.Add(filesPaths[i]);
            }
        }

        int fbxCount = fbxFilesPaths.Count;
        if (fbxCount == 0)
        {
            Debug.LogError("Folder no fbx");
            return;
        }

        for (int i = 0; i < fbxCount; i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilesPaths[i]);
            AnimationClip fbxAnimClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbxFilesPaths[i]);
            if (fbxAnimClip)
            {
                string goName = go.name;
                DuplicateAnimationClip(fbxAnimClip, folderPath + "/", goName);
                EditorUtility.DisplayProgressBar("正在复制AnimationClip...", goName, (float)i / fbxCount);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// Ctrl+D 一个.fbx中的.anim出来 changeName 是否改名
    /// </summary>
    private static void DuplicateAnimationClip(AnimationClip fbxAnimClip, string folderPath, string goName, bool changeName = false)
    {
        if (fbxAnimClip == null)
        {
            Debug.LogError("Nothing to duplicate");
            return;
        }

        AnimationClip newAnimClip = new AnimationClip();
        EditorUtility.CopySerialized(fbxAnimClip, newAnimClip);

        string newAnimClipName = folderPath + fbxAnimClip.name;
        if (changeName)//有些名字明确的不需要改名
        {
            newAnimClipName = folderPath + goName;//拼接复制动画的名字
        }
        newAnimClipName += ".anim";//接上后缀

        AssetDatabase.CreateAsset(newAnimClip, newAnimClipName);
        AssetDatabase.Refresh();

        Debug.Log(fbxAnimClip + "  SUCCESS!");
    }

    #endregion

    #region 模型碰撞

    public static string Keyword;

    //给石头 绳子添加Collider
    public static void AddCollider(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return;
        }

        var slcGos = Selection.gameObjects;
        if (slcGos == null || slcGos.Length == 0)
        {
            return;
        }

        var slcGosList = slcGos.ToList();
        slcGosList.ForEach((go) =>
        {
            if (go.GetComponent<Renderer>())
            {
                string matName = go.GetComponent<Renderer>().sharedMaterial.name;
                if (matName.Contains(keyword))
                {
                    if (go.GetComponent<MeshCollider>())
                    {
                        go.AddComponent(typeof(MeshCollider));
                    }
                }
            }
        });
        Debug.Log("Complete");
    }

    public static string NewName;
    /// <summary>
    /// 对子物体重命名
    /// </summary>
    public static void RenameChildGos(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            return;
        }

        var slcGos = Selection.activeTransform;
        if (slcGos == null)
        {
            return;
        }

        for (int i = 0; i < slcGos.childCount; i++)
        {
            slcGos.GetChild(i).name = newName + i;
        }
    }

    #endregion

    #region 资源选择

    /// <summary>
    /// 选择单个的资源（没有子物体的）
    /// </summary>
    [MenuItem("Assets/Wonderland6627/ModelTools/选择一堆asset，只保留选中单体资源")]
    public static void SelectSingleAssets()
    {
        var selectGos = Selection.objects;
        if (selectGos == null || selectGos.Length <= 1)
        {
            Debug.Log("选择为空或太短");

            return;
        }

        Debug.Log("选中物体数量 " + selectGos.Length);
        List<Object> selectObjsList = selectGos.ToList();
        List<Object> singleObjsList = new List<Object>();
        for (int i = 0; i < selectObjsList.Count; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectObjsList[i]);
            Object[] assetsArray = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assetsArray != null && assetsArray.Length > 0)
            {
                if (assetsArray.Length == 1)
                {
                    singleObjsList.Add(selectObjsList[i]);
                }
            }
        }

        Selection.objects = singleObjsList.ToArray();
    }

    #endregion
}

public class ModelToolsWindow : EditorWindow
{
    [MenuItem("Wonderland6627/ModelTools/ModelToolsWindow", priority = 0)]
    public static void OpenModelToolsWindow()
    {
        ModelToolsWindow mWindow = GetWindow<ModelToolsWindow>("ModelToolsWindow");
        mWindow.Show();
    }

    private void OnGUI()
    {
        ModelTools.changeName = GUILayout.Toggle(ModelTools.changeName, "是否改名");

        if (GUILayout.Button("选择一个fbx，复制它的动画片段出来"))
        {
            ModelTools.DuplicateModel_sAnimationClip();
        }

        if (GUILayout.Button("选择一个fbx，复制它所有的动画片段出来"))
        {
            ModelTools.DuplicateModel_sAnimationClips();
        }

        if (GUILayout.Button("选择一个文件夹，复制它下面所有fbx的动画片段出来"))
        {
            ModelTools.DuplicateModels_sAnimationClips();
        }

        if (GUILayout.Button("选择很多文件，选中其中没有子物体的部分"))
        {
            ModelTools.SelectSingleAssets();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Shader名称关键词");
        ModelTools.Keyword = EditorGUILayout.TextField(ModelTools.Keyword);
        if (GUILayout.Button("添加碰撞器"))
        {
            ModelTools.AddCollider(ModelTools.Keyword);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("设置子物体名称");
        ModelTools.NewName = EditorGUILayout.TextField(ModelTools.NewName);
        if (GUILayout.Button("设置子物体名称"))
        {
            ModelTools.RenameChildGos(ModelTools.NewName);
        }
        EditorGUILayout.EndHorizontal();
    }
}