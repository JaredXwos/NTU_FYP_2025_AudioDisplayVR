using UnityEngine;

public interface IHasVelocity
{
    Vector3 Velocity { get; }
}
[DefaultExecutionOrder(0)]
public class TransformVelocity : MonoBehaviour, IHasVelocity
{
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float smoothingTime = 0.1f; // seconds

    private Vector3 pastPosition;
    private bool initialized;

    public Vector3 Velocity => velocity;

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 1e-6f) return;

        Vector3 currentPosition = transform.position;

        if (!initialized)
        {
            pastPosition = currentPosition;
            velocity = Vector3.zero;
            initialized = true;
            return;
        }

        Vector3 rawVelocity = (currentPosition - pastPosition) / dt;

        // Exponential smoothing (first-order low-pass)
        float alpha = dt / (smoothingTime + dt);
        velocity = Vector3.Lerp(velocity, rawVelocity, alpha);

        pastPosition = currentPosition;
    }
}