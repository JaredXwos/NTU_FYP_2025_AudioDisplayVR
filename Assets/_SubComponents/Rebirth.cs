using System;
using UnityEngine;

public class RebirthEventHandler<component> : Handler<(component component, object)> where component : CoreComponent
{
    public RebirthEventHandler(Action<(component component, object)> handler, string identifier = "Unknown Rebirth Event Handler") : base(handler, identifier)
    {
    }
}

public class Rebirth : Dispatch
{
    private GameObject savedTemplate;
    private volatile bool armegeddon = false;
    private CoreComponent Parent;
    public override Type HandlerType { get; protected set; }
    protected override Type PayloadType { get; set; }

    protected override void Awake()
    {
        Parent = GetComponent<CoreComponent>();

        HandlerType = Parent != null
            ? typeof(RebirthEventHandler<>).MakeGenericType(Parent.GetType())
            : typeof(RebirthEventHandler<>).MakeGenericType(typeof(object));
        PayloadType = Parent != null
            ? typeof(ValueTuple<,>).MakeGenericType(Parent.GetType(), typeof(object))
            : typeof(ValueTuple<,>).MakeGenericType(typeof(object), typeof(object));
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        // Store a deep clone of this object as a "save point"
        savedTemplate = Instantiate(gameObject, transform.position, transform.rotation);
        savedTemplate.SetActive(false);
        foreach(Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;
        savedTemplate.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnApplicationQuit() => armegeddon = true;

    protected override void OnDestroy()
    {
        if (savedTemplate != null && !armegeddon)
        {
            Debug.Log($"[Respawn] Activated template {savedTemplate.name} at {savedTemplate.transform.position}");
            foreach (Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = true;
            foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = true;
            savedTemplate.layer = LayerMask.NameToLayer("Default");
            savedTemplate.SetActive(true);
            Invoke(Activator.CreateInstance(PayloadType, new object[] { Parent, null }));
        }
        base.OnDestroy();
    }
}