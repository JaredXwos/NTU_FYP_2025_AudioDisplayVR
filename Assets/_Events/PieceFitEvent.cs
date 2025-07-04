using System.Linq;
using UnityEngine;

public interface IHandlePieceFitEvent : IHandleEvent<Piece> { }

public class PieceFitEvent : MonoBehaviour, IRequirePieceInfo
{
    [SerializeField] private GroundSonar sonar;
    [SerializeField] private MonoBehaviour[] listeners;

    private EventBroadcaster<IHandlePieceFitEvent, Piece> broadcaster;

    private void Awake()
    {
        sonar ??= GetComponent<GroundSonar>() ?? FindFirstObjectByType<GroundSonar>();
        if(sonar == null)
        {
            enabled = false;
            Debug.LogWarning("[Piece Fit Checker] No Ground Sonar found. Disabling.");
            return;
        }
        broadcaster = new(listeners);
    }

    private void Update()
    {
        if (sonar.GetGroundClearance().All(h => h == 0)) broadcaster.InvokeEvent(sonar.Parent);
    }

}