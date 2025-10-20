using UnityEngine;

public class SetAsRootTransform : MonoBehaviour
{
    private void Update()
    {
        transform.root.position = transform.position;
        transform.root.rotation = transform.rotation;
    }
}