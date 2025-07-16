using UnityEngine;

[RequireComponent (typeof(CoreComponent))]
public class WeakenOnFit : WeakenOn<FitEventHandler<CoreComponent>, (CoreComponent piece, GameObject gameObject)> { }