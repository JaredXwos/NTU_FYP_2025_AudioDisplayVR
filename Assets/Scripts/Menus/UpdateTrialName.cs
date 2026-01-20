using TMPro;
using UnityEngine;

public class UpdateTrialName : MonoBehaviour
{
    public static string TrialName = "DefaultTrial";
    [SerializeField] private TMP_InputField inputField;
    private void Update() => TrialName = inputField.text;
}