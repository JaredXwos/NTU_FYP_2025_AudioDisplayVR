using System;
using UnityEngine;

[RequireComponent(typeof(CoreComponent))]
public class FreezeOnFit : MonoBehaviour, IHas<FitEventHandler<CoreComponent>>, IRequireInfo<CoreComponent>
{
    FitEventHandler<CoreComponent> IHas<FitEventHandler<CoreComponent>>.Handler => new(
        payload => CanBeMovedSetter(false));

    public Action<bool> CanBeMovedSetter { get; set; }
}