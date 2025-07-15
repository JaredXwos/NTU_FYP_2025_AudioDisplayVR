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
        handler = new(
            payload =>
                {
                    if (payload is IPieceCollidable p)
                    {
                        p.SetPieceCollisionEnabled(ReferenceEquals(p, activeCoreComponent));
                        Debug.Log($"Comparison: {activeCoreComponent.gameObject.name} is {payload.gameObject.name}? {ReferenceEquals(p, activeCoreComponent)}");
                    }

                },
            $"{GetType()} on {gameObject.name}"
        );

    }

    GrabEventHandler handler;
    GrabEventHandler IHas<GrabEventHandler>.Handler => handler;
}