using UnityEngine;
[DefaultExecutionOrder(-1000)]
public class TransforByDroneInput : MonoBehaviour, ICollidedState
{
    [SerializeField] private Vector3 previousDroneInputPosition;
    [SerializeField] private Vector3 deltaPosition;
    [SerializeField] private bool isCollided;
    [SerializeField] private float radius;
    public bool IsCollided => isCollided;


    private IDroneInputInterface DroneInput;
    private void Awake() => Check.PropertyEnabledElseAssign<IDroneInputInterface>(this, "DroneInput");
    private void Update()
    {
        Vector3 currentDroneInputPosition = new(
            -(float)DroneInput.Y,
            (float)DroneInput.Z,
            (float)DroneInput.X
        );

        deltaPosition = currentDroneInputPosition - previousDroneInputPosition;
        isCollided = Physics.SphereCast(transform.root.position, radius, deltaPosition.normalized, out _, deltaPosition.magnitude);
        if(isCollided) deltaPosition *= 0f;

        transform.root.SetPositionAndRotation(
            transform.root.position + deltaPosition,
            Quaternion.Euler(
                (float)DroneInput.Theta * Mathf.Rad2Deg,
                -(float)DroneInput.Psi * Mathf.Rad2Deg,
                (float)DroneInput.Phi * Mathf.Rad2Deg
            )
        );

        previousDroneInputPosition = currentDroneInputPosition;
    }

    private void OnDrawGizmos()
    {
        // Only draw when we have a meaningful delta
        if (deltaPosition == Vector3.zero)
            return;

        Vector3 start = transform.root.position;
        Vector3 end = start + deltaPosition;

        // Line color: green = free, red = blocked
        Gizmos.color = isCollided ? Color.red : Color.green;

        // Draw path line
        Gizmos.DrawLine(start, end);

        // Draw sphere at start
        Gizmos.DrawWireSphere(start, radius);

        // Draw sphere at end (planned destination)
        Gizmos.DrawWireSphere(end, radius);
    }
}