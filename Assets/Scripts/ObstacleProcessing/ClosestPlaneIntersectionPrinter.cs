using UnityEngine;

public class ClosestPlaneIntersectionPrinter : MonoBehaviour
{
    public ClosestPlaneIntersection intersectionDetector;
    private void Update()
    {
        Debug.Log(intersectionDetector.NearestPoint);
    }
}