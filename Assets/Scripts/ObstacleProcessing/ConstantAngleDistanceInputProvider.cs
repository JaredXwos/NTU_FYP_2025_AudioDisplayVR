using UnityEngine;

public class ConstantObstacleSignalInputProvider : MonoBehaviour, IHasObstacleSignal
{
    [SerializeField, Range(0f, 10f)] private float distance = 1f;
    [SerializeField] private float[] directions;

    public ObstacleSignal ObstacleSignal => new(distance, directions);
}