using System;
using UnityEngine;

[RequireComponent(typeof(CoreComponent))]
public class FreezeOnFit : FreezeOn<FitEvent, FitEventPayload> { }