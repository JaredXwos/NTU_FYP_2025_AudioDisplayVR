using System;
using UnityEngine;

[RequireComponent(typeof(CoreComponent))]
public class FreezeOnFit : MonoBehaviour, IHas<FitEventHandler<CoreComponent>>, IRequireInfo<CoreComponent>
{
    [SerializeField] private CoreComponent Parent;

    private void Awake()
    {
        if(!Check.PropertyEnabledElseAssign<CoreComponent>(this, "Parent"))
        {
            Debug.LogWarning($"[Freeze On Fit] No Parent found. Disabling.");
            enabled = false;
            return;
        }
    }

    FitEventHandler<CoreComponent> IHas<FitEventHandler<CoreComponent>>.Handler => new(
        payload => {
            if (ReferenceEquals(payload.Item1, Parent)) CanBeMovedSetter(false);

    });

    public Action<bool> CanBeMovedSetter { get; set; }
}