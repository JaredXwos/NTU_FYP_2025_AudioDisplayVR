using UnityEngine;

public interface IHasVelocity
{
    Vector3 Velocity { get; }
}

public class TransformVelocity : MonoBehaviour, IHasVelocity
{
    [SerializeField] Vector3 velocity;
    private Vector3 pastPosition;
    public Vector3 Velocity => velocity;

    private void Update()
    {
        // Compute displacement over the last frame
        Vector3 currentPosition = transform.position;
        velocity = (currentPosition - pastPosition) / Time.deltaTime;

        // Update the stored position
        pastPosition = currentPosition;
    }

}