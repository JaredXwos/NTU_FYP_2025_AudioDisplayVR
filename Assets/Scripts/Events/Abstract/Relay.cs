using System;
using UnityEngine;

public abstract class EventRelay<EVENT, PAYLOAD> : Dispatch, IHas<EventHandler<EVENT, PAYLOAD>>
{
    protected EventHandler<EVENT, PAYLOAD> Handler;
    EventHandler<EVENT, PAYLOAD> IHas<EventHandler<EVENT, PAYLOAD>>.Handler => Handler;
    protected override void Awake()
    {
        Handler = (EventHandler<EVENT, PAYLOAD>) Activator.CreateInstance(
            typeof(EventHandler<EVENT, PAYLOAD>),
            (Action<PAYLOAD>)(p => OnInvoke(p)),
            $"{GetType()} on {gameObject.name}"
        );
        base.Awake();
    }

    protected virtual void Destroy() => Handler.Dispose();
    protected abstract void OnInvoke(PAYLOAD payload);
}

public record DeathOnPayload : EventPayload, IPParentCoreComponent
{
    public CoreComponent Parent { get; }
    public DeathOnPayload(CoreComponent parent)
    {
        Parent = parent;
    }
}

public abstract class DeathOn<EVENT,PAYLOAD> : EventRelay<EVENT, PAYLOAD> where PAYLOAD : IPParentCoreComponent
{
    [SerializeField] protected CoreComponent Parent = null;
    protected override void Awake()
    {
        Check.PropertyEnabledElseAssign<CoreComponent>(this, "Parent");
        EventType = typeof(DeathEvent);
        PayloadType = typeof(DeathOnPayload);
        base.Awake();
    }
    protected override void OnInvoke(PAYLOAD payload)
    {
        if(
            payload != null && payload.Parent != null &&
            this != null && this.enabled &&
            ReferenceEquals(payload.Parent.gameObject, gameObject)
        )
        {
            Invoke(new DeathOnPayload(Parent));
            Destroy(gameObject);
        }
    }
}

