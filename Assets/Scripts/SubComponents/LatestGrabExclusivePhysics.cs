using UnityEngine;

public class LatestGrabExclusivePhysics : MonoBehaviour, IHas<EventHandler<GrabEvent, IPParentCoreComponent>>
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
        Handler = new(
            payload =>
                {
                    if (this != null && this.enabled &&
                    payload != null && payload.Parent != null && 
                    payload.Parent is IPieceCollidable p && activeCoreComponent is IPieceCollidable a &&
                    activeCoreComponent is IGrabbable g && g.CanBeMoved)
                        a.SetPieceCollisionEnabled(ReferenceEquals(p, a));
                },
            this == null? "Destroyed Latest Grab Exclusive Physics" : $"Latest-grab Excl. Phys. on {gameObject.name}"
        );
    }

    EventHandler<GrabEvent, IPParentCoreComponent> IHas<EventHandler<GrabEvent, IPParentCoreComponent>>.Handler => Handler;
    protected EventHandler<GrabEvent, IPParentCoreComponent> Handler;
}