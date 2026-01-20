using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelection : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown sceneDropdown;
    [SerializeField] private StringString[] SceneNames;


    private void Awake()
    {
        HashSet<string> buildScenes = Enumerable
        .Range(0, SceneManager.sceneCountInBuildSettings)
        .Select(i => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)))
        .ToHashSet();

        // Filter SceneNames in-place
        SceneNames = SceneNames
            .Where(s =>
            {
                if (!buildScenes.Contains(s.Value))
                {
                    Debug.LogWarning(
                        $"Scene '{s.Value}' (\"{s.Key}\") is not in Build Settings and will be ignored."
                    );
                    return false;
                }
                return true;
            })
            .ToArray();

        // Populate dropdown
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(SceneNames.Select(s => s.Key).ToList());
        sceneDropdown.RefreshShownValue();
    }
    public void OnGoPressed()
    {
        string sceneName = SceneNames[sceneDropdown.value].Value;
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}