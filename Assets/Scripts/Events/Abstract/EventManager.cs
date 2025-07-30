using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected abstract HashSet<Type> ValidHandlerSignatures { get; }
    protected Dictionary<Type, object> Events = new();
    protected virtual void Awake()
    {
        foreach( Type Event in InterfaceRegistry<Dispatch>.All
            .Select(dispatcher => dispatcher.HandlerType)
            .Where(handlerType => ValidHandlerSignatures
                .Any(signature => Check.GetCompatibleTypes(handlerType).Contains(signature))
            )
            .Select(handlerType => handlerType.GetGenericArguments()[0]))
            CreateHandler(Event);
        InterfaceRegistry<EventManager>.Register(this);
    }

    protected virtual void OnDestroy()
    {
        foreach(object handler in Events.Values) if(handler is IDisposable disposable) disposable.Dispose();
        InterfaceRegistry<EventManager>.Unregister(this);
    }

    protected abstract void Manage(Type eventtype, object payload);

    public void CreateHandler(Type Event)
    {
        if (Events.ContainsKey(Event)) return;
        Events[Event] = Activator.CreateInstance(
                typeof(EventHandler<,>).MakeGenericType(Event, typeof(object)),
                (Action<object>)(p => Manage(Event, p)),
                $"Event Manager"
            );
    }
    
    public void DeleteHandler(Type Event)
    {
        if (
            InterfaceRegistry<Dispatch>.All
                .Select(dispatcher => dispatcher.HandlerType)
                .Where(handlerType => ValidHandlerSignatures
                    .Any(signature => Check.GetCompatibleTypes(handlerType).Contains(signature))
                )
                .Contains(Event)
        ) return;
        if (!Events.TryGetValue(Event, out var handler))
            return;
        if (handler is IDisposable disposable ) disposable.Dispose();
        Events.Remove(Event);
    }
    
}