using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class EventResponder<EVENT, PAYLOAD> : MonoBehaviour, IHas<Handler<EVENT, PAYLOAD>>
{
    Handler<EVENT, PAYLOAD> Handler;
    Handler<EVENT, PAYLOAD> IHas<Handler<EVENT, PAYLOAD>>.Handler => Handler;

    protected virtual void Awake() => Handler = (Handler<EVENT, PAYLOAD>) Activator.CreateInstance(
        typeof(Handler<EVENT, PAYLOAD>),
        (Action<PAYLOAD>) (p =>  OnInvoke(p)),
        $"{GetType()} on {gameObject.name}"
    );

    protected virtual void OnDestroy() => Handler.Dispose();

    protected abstract void OnInvoke(PAYLOAD payload);
}

public abstract class ParentAwareEventResponder<EVENT, PAYLOAD> : EventResponder<EVENT, PAYLOAD> where PAYLOAD : IPParentCoreComponent
{
    [SerializeField] protected CoreComponent Parent = null;
    protected override void Awake()
    {
        Check.PropertyEnabledElseAssign<CoreComponent>(this, "Parent");
        base.Awake();
    }
    protected virtual bool IsInvolved(PAYLOAD payload) => 
        this != null && enabled &&
        payload != null && payload.Parent != null &&
        ReferenceEquals(payload.Parent, Parent);
}

public class ReconstructOn<EVENT, PAYLOAD> : ParentAwareEventResponder<EVENT, PAYLOAD>  where PAYLOAD : IPParentCoreComponent
{
    private GameObject savedTemplate;
    private volatile bool armegeddon = false;

    protected virtual void Start()
    {
        // Store a deep clone of this object as a "save point"
        savedTemplate = Instantiate(gameObject, transform.position, transform.rotation);
        savedTemplate.SetActive(false);
        foreach (Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;
        savedTemplate.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    protected virtual void OnApplicationQuit() => armegeddon = true;

    protected override void OnInvoke(PAYLOAD payload)
    {
        if (savedTemplate != null && !armegeddon && 
            IsInvolved(payload))
        {
            Debug.Log($"[Respawn] Activated template {savedTemplate.name} at {savedTemplate.transform.position}");
            foreach (Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = true;
            foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = true;
            savedTemplate.layer = LayerMask.NameToLayer("Default");
            savedTemplate.SetActive(true);
        }
    }
}

public abstract class AddMonoBehaviourOn<EVENT, PAYLOAD, COMPONENT> : ParentAwareEventResponder<EVENT, PAYLOAD> where PAYLOAD : IPParentCoreComponent where COMPONENT : MonoBehaviour
{
    [SerializeField] protected bool SingleUse;
    protected override void OnInvoke(PAYLOAD payload)
    {
        if (IsInvolved(payload))
        {
            Debug.Log($"Parent: {Parent}, Payload Parent {payload.Parent} {ReferenceEquals(payload.Parent, Parent)}");
            gameObject.AddComponent<COMPONENT>();
            if (SingleUse) Destroy(this);
        }
    }
}

