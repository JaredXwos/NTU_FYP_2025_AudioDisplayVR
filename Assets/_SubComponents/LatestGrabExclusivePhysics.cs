using UnityEngine;

public class LatestGrabExclusivePhysics : MonoBehaviour, IHas<GrabEventHandler>
{
    [SerializeField] private CoreComponent activeCoreComponent;

    private void Awake()
    {
        if (this != null && !Check.PropertyEnabledElseAssign<CoreComponent>(this, "activeCoreComponent"))
        {
            Debug.LogWarning("[LatestGrabExclusivePhysics] No attached core component, disabling");
            enabled = false;
            return;
        }
        handler = new(
            payload =>
                {
                    if (this != null && payload != null && payload is IPieceCollidable p)
                    {
                        p.SetPieceCollisionEnabled(ReferenceEquals(p, activeCoreComponent));
                    }
                },
            this == null? "Destroyed Latest Grab Exclusive Physics" : $"Latest Grab Exclusive Physics on {gameObject.name}"
        );

    }

    GrabEventHandler handler;
    GrabEventHandler IHas<GrabEventHandler>.Handler => handler;
}