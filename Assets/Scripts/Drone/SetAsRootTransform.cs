using UnityEngine;

public class SetAsRootTransform : MonoBehaviour
{
    private void Update()
    {
        transform.root.SetPositionAndRotation(transform.position, transform.rotation);
    }
}