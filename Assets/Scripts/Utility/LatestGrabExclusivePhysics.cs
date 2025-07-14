using UnityEngine;

public class LatestGrabExclusivePhysics : MonoBehaviour, IHas<GrabEventHandler>
{
    [SerializeField] private CoreComponent activeCoreComponent;

    private void Awake()
    {
        if (!Check.PropertyEnabledElseAssign<CoreComponent>(this, "activeCoreComponent"))
        {
            Debug.LogWarning("[LatestGrabExclusivePhysics] No attached core component, disabling");
            enabled = false;
            return;
        }
            
    }

    GrabEventHandler IHas<GrabEventHandler>.Handler => new(
        payload =>
        {
            if (payload is IPieceCollidable p)
                p.SetPieceCollisionEnabled(ReferenceEquals(p, activeCoreComponent));
        }
    );
}