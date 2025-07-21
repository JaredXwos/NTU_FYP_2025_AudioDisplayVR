using UnityEngine;

public class LatestGrabExclusivePhysics : MonoBehaviour, IHas<Handler<GrabEvent, IPParentCoreComponent>>
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
                    payload.Parent is IPieceCollidable p && activeCoreComponent is IPieceCollidable a)
                        a.SetPieceCollisionEnabled(ReferenceEquals(p, a));
                },
            this == null? "Destroyed Latest Grab Exclusive Physics" : $"Latest-grab Excl. Phys. on {gameObject.name}"
        );
    }

    Handler<GrabEvent, IPParentCoreComponent> IHas<Handler<GrabEvent, IPParentCoreComponent>>.Handler => Handler;
    protected Handler<GrabEvent, IPParentCoreComponent> Handler;
}