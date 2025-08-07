using UnityEngine;

[RequireComponent (typeof(CoreComponent))]
public class WeakenOnFit : WeakenOn<FitEvent,IPParentCoreComponent> { }