using UnityEngine;

public record EventPayload { };

public interface IPParentCoreComponent
{
    CoreComponent Parent { get; }
}

public interface IPCaller<T> where T : MonoBehaviour
{
    T Caller { get; }
}

public interface IPActive
{
    bool IsActive { get; }
}

public interface IPCollidee
{
    GameObject Collidee { get; }
}

public interface IPCollidees
{
    GameObject[] Collidees { get; }
}