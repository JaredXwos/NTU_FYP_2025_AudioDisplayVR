using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Raycaster : MonoBehaviour
{
    [SerializeField] private int downwardDistance = -1;
    [SerializeField] private int distanceLimit = 10;
    [SerializeField] private new Collider collider;

    private void Awake() => collider = collider != null ? collider : GetComponent<Collider>();
    public int GetDownwardRaycastDistance(Quaternion rotation)
    {
        Vector3 origin = collider.bounds.center + Vector3.down * (collider.bounds.extents.y - 0.01f);
        downwardDistance = Physics.Raycast(origin, rotation * Vector3.down, out RaycastHit hit, distanceLimit)?
            (int) hit.distance : -1;
        return downwardDistance;
    }
}
