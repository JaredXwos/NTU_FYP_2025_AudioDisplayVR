using UnityEngine;

public class WeakenOnFit : EventCascade<FitEventHandler<Piece>, (Piece piece, GameObject gameObject), IWeaken>
{
    protected override void Dispatch(IWeaken component) => component.Weaken();
    protected override bool Condition((Piece piece, GameObject gameObject) payload) => payload.piece == GetComponent<Piece>();
}