using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour, IHas<Handler<FitEvent,object>>
{
    [SerializeField] private TextMeshProUGUI statusText = default;
    [SerializeField] private int score = 0;


    #region MonoBehaviour
    private void Awake()
    {
        Handler = new(
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
    Handler<FitEvent, object> Handler;
    Handler<FitEvent, object> IHas<Handler<FitEvent, object>>.Handler => Handler;
    #endregion
}