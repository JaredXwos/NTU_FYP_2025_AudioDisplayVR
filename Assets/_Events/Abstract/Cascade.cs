using System;
using UnityEngine;

public abstract class EventCascade<EVENT, PAYLOAD, COMPONENT> : MonoBehaviour, IHas<Handler<EVENT, PAYLOAD>>
{
    protected Handler<EVENT, PAYLOAD> Handler;
    Handler<EVENT, PAYLOAD> IHas<Handler<EVENT, PAYLOAD>>.Handler => Handler;

    protected virtual void Awake() => Handler = (Handler<EVENT, PAYLOAD>) Activator.CreateInstance(
        typeof(Handler<EVENT, PAYLOAD>),
        (Action<PAYLOAD>)(p => {
            if (ShouldCascade(p))
                foreach (COMPONENT component in GetComponentsInChildren<COMPONENT>())
                    Dispatch(component);
        }),
        $"{GetType()} on {gameObject.name}"
    );
    protected virtual void OnDestroy() => Handler.Dispose();

    protected abstract void Dispatch(COMPONENT component);
    protected virtual bool ShouldCascade(PAYLOAD payload) => true;
}

public abstract class ParentAwareEventCascade<EVENT, PAYLOAD, COMPONENT> : EventCascade<EVENT, PAYLOAD, COMPONENT> where PAYLOAD : IPParentCoreComponent
{
    [SerializeField] protected bool RequireInvolvement;
    [SerializeField] protected CoreComponent Parent;

    protected override void Awake()
    {
        Parent = GetComponent<CoreComponent>();
        base.Awake();
    }
    protected override bool ShouldCascade(PAYLOAD payload) =>
        this != null && enabled && 
        (!RequireInvolvement || 
        payload != null && payload.Parent != null && 
        ReferenceEquals(payload.Parent, Parent));
}

public abstract class WeakenOn<EVENT, PAYLOAD>: ParentAwareEventCascade<EVENT, PAYLOAD, IWeaken> where PAYLOAD : IPParentCoreComponent
{
    protected override void Dispatch(IWeaken component) => component.Weaken();
}

public abstract class FreezeOn<HANDLER, PAYLOAD>
    : ParentAwareEventCascade<HANDLER, PAYLOAD, IGrabbable>,
    IRequireAuthorisation<IGrabbable>
    where PAYLOAD : IPParentCoreComponent
{
    public object Key { protected get; set; }
    protected override void Dispatch(IGrabbable component)
    {
        try { component.SetCanBeMoved(Key, false); }
        catch (InvalidOperationException)
        {
            component.Authenticate();
            component.SetCanBeMoved(Key, false);
        }
    }

}

public abstract class RefreshOn<HANDLER, PAYLOAD>
    : ParentAwareEventCascade<HANDLER, PAYLOAD, IRefresh> where PAYLOAD: IPParentCoreComponent
{
    protected override void Dispatch(IRefresh component) => component.Refresh();
}


