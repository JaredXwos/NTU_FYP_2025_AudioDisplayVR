using UnityEngine;

public class PrintAngleDistance : MonoBehaviour
{
    [SerializeField] ClosestObstacleAngleDistanceInputProvider inputProvider;
    private void Update()
    {
        if (inputProvider == null) return;
        float angleDeg = inputProvider.Angle * Mathf.Rad2Deg;
        float distance = inputProvider.Distance;
        Debug.Log($"Closest Obstacle - Angle: {angleDeg:F1} deg, Distance: {distance:F2} m");
    }
}