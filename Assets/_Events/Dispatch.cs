using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public abstract class Dispatch : MonoBehaviour
{
    // HANDLERS
    // -----------------------------------------------------------------------------------
    // List of identifiers (typically names or labels) of the registered handlers.
    [SerializeField] List<string> listeners;
    // Stores handlers and lambda delegates
    private readonly Dictionary<object, Action<object>> invokes = new();

    // SUBCLASS DEFINED TYPES
    // -----------------------------------------------------------------------------------
    // The main handler type this dispatcher accepts, defined by subclass
    public abstract Type HandlerType { get; protected set; }
    // The type of payload this dispatcher will pass to handlers, defined by subclass
    protected abstract Type PayloadType { get; set; }

    // LAMBDA CONSTRUCTION
    // -----------------------------------------------------------------------------------
    // Cached set of all types compatible with HandlerType (including interfaces, base types, and generic combinations)
    private HashSet<Type> compatibleHandlerTypes;
    // Argument of the constructed lambda
    private readonly ParameterExpression lambdaArgument = Expression.Parameter(typeof(object), "payload");
    // Parameter of the constructed lambda
    private UnaryExpression lambdaParameter;

    protected virtual void Awake()
    {
        if(!
            typeof(Handler<>).MakeGenericType(PayloadType)
            .IsAssignableFrom(HandlerType))
            throw new InvalidOperationException($"Handler type {HandlerType} is not compatible with Handler<{PayloadType}>");
        compatibleHandlerTypes = Check.GetCompatibleTypes(HandlerType);
        lambdaParameter = Expression.Convert(lambdaArgument, PayloadType);
    }

    protected virtual void Start()
    {
        BuildInvokes();
        typeof(InterfaceRegistry<>)
            .MakeGenericType(typeof(Dispatch))
            .GetMethod("Register", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { this });
    }

    protected virtual void OnDestroy()
    {
        typeof(InterfaceRegistry<>)
            .MakeGenericType(typeof(Dispatch))
            .GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { this });
    }

    protected void Invoke(object payload)
    {
        if (!PayloadType.IsInstanceOfType(payload))
            throw new InvalidCastException($"[Dispatch::{GetType().Name}] Expected payload of type {PayloadType}, but got {payload?.GetType()}");

        foreach (var (handler, action) in invokes)
        {
            if (handler is UnityEngine.Object uo && !uo)
                continue;

            action(payload);
        }
    }

    public void CompileInvoke(object Handler)
    {
        if (compatibleHandlerTypes.Contains(Handler.GetType()))
        {
            if (invokes.ContainsKey(Handler))
            {
                Debug.LogWarning($"[CompileInvoke] Handler already registered: {Handler}");
                return;
            }

            string identifier = Handler
                .GetType()
                .GetProperty("Identifier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                ?.GetValue(Handler) as string ?? "Unknown";

            listeners.Add(identifier);

            PropertyInfo handleProp = Handler
                .GetType()
                .GetProperty("Handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                ?? throw new InvalidOperationException($"Handler lacks Handle on {identifier}.");

            System.Object handleDelegate = handleProp
                .GetValue(Handler)
                ?? throw new InvalidOperationException($"Handler.Handle returned null on {identifier}.");

            Type expectedParameterType = handleDelegate
                .GetType()
                .GetMethod("Invoke")
                .GetParameters()[0].ParameterType;

            InvocationExpression invokeCall = Expression.Invoke(
                Expression.Constant(handleDelegate),
                Check.BuildCompatibleNewInstance(lambdaParameter, lambdaParameter.Type, expectedParameterType)
            );


            Action<object> compiled = Expression.Lambda<Action<object>>(invokeCall, lambdaArgument).Compile();

            invokes[Handler] = payload =>
            {
                if (Handler is UnityEngine.Object uo && !uo) return;
                compiled(payload);
            };
        }
        else
        {
            Debug.LogWarning($"[Compile Invoke] Handler Type {Handler.GetType()} Not Compatible");
            foreach (Type type in compatibleHandlerTypes) Debug.Log(type.FullName);
        }
    }

    public void DeleteInvoke(object Handler)
    {
        invokes.Remove(Handler);
        listeners.Remove(
            Handler
            .GetType()
            .GetProperty("Identifier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
            ?.GetValue(Handler) as string ?? "Unknown"
        );
    }

    private void BuildInvokes()
    {

        List<object> compatibleHandlers = new();
        foreach (Type handlerType in compatibleHandlerTypes)
        {

            IReadOnlyCollection<object> classHandlers = (IReadOnlyCollection<object>) typeof(InterfaceRegistry<>)
                .MakeGenericType(handlerType)
                .GetProperty("All", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            foreach (var handler in classHandlers)
                compatibleHandlers.Add(handler);
        }

        foreach(var handler in compatibleHandlers) CompileInvoke(handler);
    }
}