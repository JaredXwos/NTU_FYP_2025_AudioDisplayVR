using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour, IHas<PieceFitEventHandler>
{
    [SerializeField] private TextMeshProUGUI statusText = default;
    [SerializeField] private int score = 0;


    #region MonoBehaviour
    private void Awake()
    {
        statusText ??= GetComponent<TextMeshProUGUI>() ?? FindFirstObjectByType<TextMeshProUGUI>();
        if(statusText == null)
        {
            enabled = false;
            Debug.LogWarning("[Status Bar] No TextMesh found. Disabling.");
            return;
        }
        statusText.text = "Score: 0";
    }
    #endregion

    #region IHandlePieceFitEvent
    public PieceFitEventHandler Handler => new(
        ((Piece piece, GameObject gameObject) _) =>
        {
            if (statusText != null) statusText.text = $"Score: {++score}";
        });
    public void HandleEvent(Piece _)
    {
        if(statusText != null) statusText.text = $"Score: {++score}";
    }
    #endregion
}