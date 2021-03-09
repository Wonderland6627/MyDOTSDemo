using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationConvertTools
{
    public static void CreateAnimationDataAsset(string dirPath, string assetName)
    {
        DirectoryInfo info = new DirectoryInfo(dirPath);
        if (!info.Exists)
        {
            info.Create();
            Debug.Log("Create Dir");
        }

        string fullName = Path.Combine(dirPath, assetName + ".asset");
        AnimationData data = ScriptableObject.CreateInstance<AnimationData>();

        AssetDatabase.CreateAsset(data, fullName);
    }

    public static void Save(AnimationClip animationClip, AnimationData dataAsset)
    {
        var path = AssetDatabase.GetAssetPath(dataAsset);
        var asset = AssetDatabase.LoadAssetAtPath<AnimationData>(path);

        asset.frameDelta = (float)(1 / 30f);
        asset.frameCount = Mathf.CeilToInt(animationClip.length / asset.frameDelta);
        asset.positions = new List<Vector3>();
        asset.rotations = new List<Vector3>();
        asset.scales = new List<Vector3>();

        for (int i = 0; i < asset.frameCount; i++)
        {
            asset.positions.Add(Vector3.zero);
            asset.rotations.Add(Vector3.zero);
            asset.scales.Add(Vector3.one);
        }

        foreach (var binding in AnimationUtility.GetCurveBindings(animationClip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);

            string propName = binding.propertyName;
            float timer = 0f;
            float maxTime = animationClip.length;
            int index = 0;

            while (timer < maxTime && index < asset.frameCount)
            {
                Debug.Log(propName);
                switch (propName)
                {
                    case "m_LocalPosition.x":
                        {
                            var pos = asset.positions[index];
                            pos.x = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;
                            break;
                        }
                    case "m_LocalPosition.y":
                        {
                            var pos = asset.positions[index];
                            pos.y = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;
                        }
                        break;
                    case "m_LocalPosition.z":
                        {
                            var pos = asset.positions[index];
                            pos.z = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;
                        }
                        break;
                }
                timer += asset.frameDelta;
                index++;
            }
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static float GetValue(Keyframe[] frames, float time)
    {
        int pre = 0;
        int next = 0;
        for (int i = 0; i < frames.Length; ++i)
        {
            var frame = frames[i];
            if (time <= frame.time)
            {
                next = i;
                break;
            }
        }
        pre = Mathf.Max(0, next - 1);

        var preFrame = frames[pre];
        var nextFrame = frames[next];

        if (pre == next)
            return nextFrame.time;

        float ret = preFrame.value + (nextFrame.value - preFrame.value) * (time - preFrame.time) / (nextFrame.time - preFrame.time);
        return ret;
    }
}

public class AnimationConvertWindow : EditorWindow
{
    [MenuItem("Wonderland6627/AnimationTools/AnimationConverWindow")]
    public static void OpenConvertWindow()
    {
        AnimationConvertWindow window = GetWindow<AnimationConvertWindow>("AnimationConvertWindow");
        window.Show();
    }

    private static AnimationClip animationClip;

    private static string assetName;
    private static AnimationData dataAsset;

    private void OnGUI()
    {
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Label("AssetName: ");
            assetName = EditorGUILayout.TextField(assetName);
            if (GUILayout.Button("CreateAsset"))
            {
                AnimationConvertTools.CreateAnimationDataAsset("Assets/Resources/AnimationDatas", assetName);
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("AnimationClip: ");
        animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;

        GUILayout.Space(10);
        GUILayout.Label("AnimationData ScriptableObject Asset: ");
        dataAsset = EditorGUILayout.ObjectField(dataAsset, typeof(AnimationData), false) as AnimationData;

        GUILayout.Space(10);
        if (GUILayout.Button("Save"))
        {
            AnimationConvertTools.Save(animationClip, dataAsset);
        }
    }
}
