using System;
using System.Linq;
using UnityEngine;

public class PieceFitEventHandler : Handler<(Piece piece, GameObject gameObject)>
{
    public PieceFitEventHandler(Action<(Piece piece, GameObject gameObject)> handler) : base(handler)
    {
    }
}

public class PieceFitEvent : MonoBehaviour
{
    [SerializeField] private GroundSonar sonar;
    [SerializeField] private MonoBehaviour[] listeners;

    private EventBroadcaster<IHas<PieceFitEventHandler>, (Piece piece, GameObject gameObject)> broadcaster;

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
        if (
            sonar.GetGroundClearance().All(h => h == 0) &&
            Physics.Raycast(transform.position, -sonar.Parent.transform.up, out RaycastHit hit, 4)
        )
        broadcaster.InvokeEvent((sonar.Parent, hit.collider.gameObject));
    }

}