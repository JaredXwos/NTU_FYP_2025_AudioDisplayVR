using System;
using UnityEngine;

[RequireComponent(typeof(CoreComponent))]
public class FreezeOnFit : FreezeOn<FitEventHandler<CoreComponent>, (CoreComponent, GameObject)> { }