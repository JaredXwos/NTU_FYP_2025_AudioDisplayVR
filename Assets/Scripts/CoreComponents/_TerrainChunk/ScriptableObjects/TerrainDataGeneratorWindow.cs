#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class TerrainDataGeneratorWindow : EditorWindow
{
    private int width = 4;
    private int length = 4;
    private string heightData = "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15";
    private string assetName = "NewTerrainData";
    private string folderPath = "Assets/Scripts/CoreComponents/_TerrainChunk/ScriptableObjects";

    [MenuItem("Tools/Terrain/Generate TerrainDataSO")]
    public static void ShowWindow() 
        => GetWindow<TerrainDataGeneratorWindow>("TerrainDataSO Generator");
    

    private void OnGUI()
    {
        GUILayout.Label("Terrain Data Generator", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        length = EditorGUILayout.IntField("Length", length);
        heightData = EditorGUILayout.TextArea(heightData, GUILayout.Height(100));
        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Generate TerrainDataSO"))
            GenerateTerrainDataSO();
    }

    private void GenerateTerrainDataSO()
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
            Directory.CreateDirectory(folderPath);

        List<float> parsedHeights = heightData
            .Split(new[] { ' ', '\n', '\r', '\t' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(s => float.TryParse(s, out var f) ? f : 0f)
            .ToList();

        var asset = ScriptableObject.CreateInstance<TerrainDataSO>();

        // Use reflection or SerializedObject to set private fields
        var serializedObject = new SerializedObject(asset);
        serializedObject.FindProperty("width").intValue = width;
        serializedObject.FindProperty("length").intValue = length;

        var heightsProp = serializedObject.FindProperty("heights");
        heightsProp.ClearArray();
        for (int i = 0; i < parsedHeights.Count; i++)
        {
            heightsProp.InsertArrayElementAtIndex(i);
            heightsProp.GetArrayElementAtIndex(i).floatValue = parsedHeights[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        asset.ValidateSize(); // to ensure correct padding

        string assetPath = Path.Combine(folderPath, assetName + ".asset");
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"[TerrainDataGenerator] Created {assetName} with {parsedHeights.Count} height values at {assetPath}");
    }
}
#endif