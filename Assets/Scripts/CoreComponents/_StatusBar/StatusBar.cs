using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour, IHas<EventHandler<FitEvent,object>>
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
    EventHandler<FitEvent, object> Handler;
    EventHandler<FitEvent, object> IHas<EventHandler<FitEvent, object>>.Handler => Handler;
    #endregion
}