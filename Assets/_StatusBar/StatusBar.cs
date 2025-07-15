using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour, IHas<FitEventHandler<Piece>>
{
    [SerializeField] private TextMeshProUGUI statusText = default;
    [SerializeField] private int score = 0;


    #region MonoBehaviour
    private void Awake()
    {
        handler = new(
            _ =>{ if (statusText != null) statusText.text = $"Score: {++score}"; },
            gameObject.name
        );
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
    FitEventHandler<Piece> handler;
    FitEventHandler<Piece> IHas<FitEventHandler<Piece>>.Handler => new(
        ((Piece piece, GameObject gameObject) _) =>
        {
            if (statusText != null) statusText.text = $"Score: {++score}";
        });
    #endregion
}