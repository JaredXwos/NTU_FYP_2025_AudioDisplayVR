using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class EventCascade<HANDLER, PAYLOAD, COMPONENT> : MonoBehaviour, IHas<HANDLER>
    where HANDLER : Handler<PAYLOAD>
{
    public HANDLER handler;
    public HANDLER Handler => handler;

    protected virtual void Awake() => handler = (HANDLER)Activator.CreateInstance(
        typeof(HANDLER),
        (Action<PAYLOAD>)(p => {
            if (Condition(p))
                foreach (COMPONENT component in GetComponentsInChildren<COMPONENT>())
                    Dispatch(component);
        }),
        $"{GetType()} on {gameObject.name}"
    );

    protected abstract void Dispatch(COMPONENT component);
    protected virtual bool Condition(PAYLOAD payload) => true;
}

public abstract class WeakenOn<HANDLER, PAYLOAD>
    : EventCascade<HANDLER, PAYLOAD, IWeaken> where HANDLER : Handler<PAYLOAD> where PAYLOAD : ITuple
{
    [Tooltip("Weaken on any fit event or only on its own fit event")]
    [SerializeField] private bool WeakenOnAny;
    protected override void Dispatch(IWeaken component) => component.Weaken();

    protected override bool Condition(PAYLOAD payload) => this != null && enabled && (WeakenOnAny || ReferenceEquals(payload[0], GetComponent<CoreComponent>()));
}

public abstract class FreezeOn<HANDLER, PAYLOAD>
    : EventCascade<HANDLER, PAYLOAD, IGrabbable>,
    IRequireAuthorisation<IGrabbable>
    where HANDLER : Handler<PAYLOAD> where PAYLOAD : ITuple
{
    [SerializeField] private bool FreezeOnAny;
    public object Key { protected get; set; }
    protected override void Dispatch(IGrabbable component) => component.SetCanBeMoved(Key, false);
    protected override bool Condition(PAYLOAD payload) => this != null && enabled && (FreezeOnAny || ReferenceEquals(payload[0], GetComponent<CoreComponent>()));

}

public abstract class RefreshOn<HANDLER, PAYLOAD>
    : EventCascade<HANDLER, PAYLOAD, IRefresh> where HANDLER : Handler<PAYLOAD> where PAYLOAD : ITuple
{
    [SerializeField] private bool RefreshOnAny;

    protected override void Dispatch(IRefresh component) => component.Refresh();
    protected override bool Condition(PAYLOAD payload) => this != null && enabled && (RefreshOnAny || payload[0] != null && ReferenceEquals(payload[0], GetComponent<CoreComponent>()));
}


