using UnityEngine;

public class LatestGrabExclusiveAudio : MonoBehaviour, IHas<EventHandler<GrabEvent, IPParentCoreComponent>>
{
    [SerializeField] private CoreComponent activeCoreComponent;

    private void Awake()
    {
        if (this != null && !Check.PropertyEnabledElseAssign<CoreComponent>(this, "activeCoreComponent"))
        {
            Debug.LogWarning("[LatestGrabExclusiveAudio] No attached core component, permenantly silencing audio");
            return;
        }
        Handler = new(
            payload =>
                {
                    if (this != null && this.enabled &&
                    payload != null && payload.Parent != null)
                        foreach (AudioGenerator_Deprecated AG in activeCoreComponent.GetComponents<AudioGenerator_Deprecated>())
                            AG.IsPlaying = ReferenceEquals(payload.Parent, activeCoreComponent);
                },
            this == null? "Destroyed Latest Grab Exclusive Audio" : $"Latest-grab Excl. Audio on {gameObject.name}"
        );
    }

    EventHandler<GrabEvent, IPParentCoreComponent> IHas<EventHandler<GrabEvent, IPParentCoreComponent>>.Handler => Handler;
    protected EventHandler<GrabEvent, IPParentCoreComponent> Handler;
}