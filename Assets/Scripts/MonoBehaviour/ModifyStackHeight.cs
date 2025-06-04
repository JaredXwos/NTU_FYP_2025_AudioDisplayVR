using UnityEngine;

public class ModifyStackHeight : MonoBehaviour
{
    [SerializeField] public int StackHeight = 0;
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;

    public int ResetHeight(int height = 0)
    {
        StackHeight = height > 0 ? 
            height : 
            Random.Range(minGeneratedHeight, maxGeneratedHeight + 1);

        transform.localScale = new Vector3(1, StackHeight, 1);
        transform.position = new Vector3(transform.position.x, transform.position.y-StackHeight / 2f, transform.position.z);

        return height;
    }
    
}
