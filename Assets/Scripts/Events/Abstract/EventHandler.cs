using System;
using UnityEngine;

public interface IHas<out T>
{
    public T Handler { get; }
}

public class EventHandler<EVENT, PAYLOAD> : IDisposable
{
    public Action<PAYLOAD> Handle { get; }
    public string Identifier { get; }

    public EventHandler(Action<PAYLOAD> handler, string identifier = "Unknown Parent")
    {
        Handle = handler;
        Identifier = identifier;

        InterfaceRegistry.Register(this);

        foreach (Dispatch dispatcher in InterfaceRegistry<Dispatch>.All) 
            if(Check.GetCompatibleTypes(dispatcher.HandlerType).Contains(GetType())) 
                dispatcher.CompileInvoke(this);
    }

    public void Dispose()
    {
        InterfaceRegistry.Unregister(this);
        foreach (Dispatch dispatcher in InterfaceRegistry<Dispatch>.All) dispatcher.DeleteInvoke(this);
    }
}

/// <summary>
/// Example of a concrete handler for object payloads. 
/// Typically you'd define this near your Dispatch class for convenience,
/// but it can live anywhere in your codebase.
/// </summary>
internal class ExampleEvent { }

/// <summary>
/// Illustrative pattern of how to structure a listener MonoBehaviour 
/// that holds a handler and implements IHas.
/// This is purely a template — not intended for actual use in gameplay.
/// </summary>
internal abstract class ExampleListener : MonoBehaviour, IHas<EventHandler<ExampleEvent, object>>
{
    private EventHandler<ExampleEvent, object> handler;
    EventHandler<ExampleEvent, object> IHas<EventHandler<ExampleEvent, object>>.Handler => throw new NotImplementedException();

    private void Awake()
    {
        // Example of creating the handler. 
        // In a real listener you would provide actual logic for handling the payload.
        handler = new EventHandler<ExampleEvent, object>(
            p => { return; },
            "Example Listener"
        );

        // Since this is only a template, we throw to prevent accidental usage.
        throw new NotImplementedException("This is only an example template. Implement in your own concrete listener.");
    }
}

