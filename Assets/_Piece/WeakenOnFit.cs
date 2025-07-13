using UnityEngine;

public class WeakenOnFit : EventCascade<FitEventHandler<CoreComponent>, (CoreComponent piece, GameObject gameObject), IWeaken>
{
    protected override void Dispatch(IWeaken component) => component.Weaken();
    protected override bool Condition((CoreComponent piece, GameObject gameObject) payload) => payload.piece == GetComponent<CoreComponent>();
}   