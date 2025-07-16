using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface IHas<out T>
{
    public T Handler { get; }
}

public class Handler<T> : IDisposable
{
    public Action<T> Handle { get; }
    public string Identifier { get; }

    public Handler(Action<T> handler, string identifier = "Unknown Parent")
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
internal class ExampleHandler : Handler<object>
{
    // Constructor simply passes through to the base Handler, registering itself.
    internal ExampleHandler(Action<object> handler, string identifier = "Example Handler")
        : base(handler, identifier) { }
}

/// <summary>
/// Illustrative pattern of how to structure a listener MonoBehaviour 
/// that holds a handler and implements IHas.
/// This is purely a template — not intended for actual use in gameplay.
/// </summary>
internal abstract class ExampleListener : MonoBehaviour, IHas<ExampleHandler>
{
    private ExampleHandler handler;
    ExampleHandler IHas<ExampleHandler>.Handler => handler;

    private void Awake()
    {
        // Example of creating the handler. 
        // In a real listener you would provide actual logic for handling the payload.
        handler = new ExampleHandler(
            p => { return; },
            "Example Listener"
        );

        // Since this is only a template, we throw to prevent accidental usage.
        throw new NotImplementedException("This is only an example template. Implement in your own concrete listener.");
    }
}

