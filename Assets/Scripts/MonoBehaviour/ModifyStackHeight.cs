using UnityEngine;

public class ModifyStackHeight : MonoBehaviour
{
    [SerializeField] public int StackHeight = 0;
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;
    [SerializeField] public float sortIndex;
    private void Awake() => sortIndex = transform.localPosition.x;
    public int ResetHeight(int height = 0)
    {
        StackHeight = height > 0 ? 
            height : 
            Random.Range(minGeneratedHeight, maxGeneratedHeight + 1);

        transform.localScale = new Vector3(1, StackHeight, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, - StackHeight / 2f, 0);
        return StackHeight;
    }
    
}
