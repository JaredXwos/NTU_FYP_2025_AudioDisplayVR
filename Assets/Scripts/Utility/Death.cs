using UnityEngine;

public class Death : MonoBehaviour
{
    private volatile bool dead = false;
    private void Update()
    { 
        if(dead) Destroy(gameObject);
    }
    public void Trigger() => dead = true;
}