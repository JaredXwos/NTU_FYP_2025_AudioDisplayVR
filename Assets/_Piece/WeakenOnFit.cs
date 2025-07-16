using UnityEngine;

[RequireComponent (typeof(CoreComponent))]
public class WeakenOnFit : EventCascade<FitEventHandler<CoreComponent>, (CoreComponent piece, GameObject gameObject), IWeaken>
{
    [Tooltip("Weaken on any fit event or only on its own fit event")]
    [SerializeField] private bool WeakenOnAny;
    protected override void Dispatch(IWeaken component) => component.Weaken();
    
    protected override bool Condition((CoreComponent piece, GameObject gameObject) payload) => this != null && enabled && (WeakenOnAny || payload.piece == GetComponent<CoreComponent>());
}   