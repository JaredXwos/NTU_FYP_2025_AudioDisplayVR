using UnityEngine;

public class FreezeOnFit : IHas<FitEventHandler<CoreComponent>>
{
    FitEventHandler<CoreComponent> IHas<FitEventHandler<CoreComponent>>.Handler => throw new System.NotImplementedException();
}