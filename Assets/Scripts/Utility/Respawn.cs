using UnityEngine;

public class Respawn : MonoBehaviour
{
    private GameObject savedTemplate;

    private void Awake()
    {
        // Store a deep clone of this object as a "save point"
        savedTemplate = Instantiate(gameObject, transform.position, transform.rotation);
        savedTemplate.SetActive(false);
    }

    private void OnDestroy()
    {
        if (enabled && savedTemplate != null)
        {
            GameObject newClone = Instantiate(savedTemplate, savedTemplate.transform.position, savedTemplate.transform.rotation);
            newClone.SetActive(true);
        }
    }
}