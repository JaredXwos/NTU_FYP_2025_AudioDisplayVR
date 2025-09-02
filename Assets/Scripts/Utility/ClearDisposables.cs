using System;
using System.Collections.Generic;
using UnityEngine;

public class ClearDisposables : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        IReadOnlyCollection<IDisposable> disposables = InterfaceRegistry<IDisposable>.All;
        foreach (IDisposable disposable in disposables)
            disposable.Dispose();
    }
}