using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider))]
public class CollisionChecker : MonoBehaviour
{
    public bool IsCollided() => Physics.CheckSphere(transform.position, 0f);
}
