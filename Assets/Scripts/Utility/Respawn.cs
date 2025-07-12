using UnityEngine;

public class Respawn : MonoBehaviour
{
    private GameObject savedTemplate;
    private volatile bool armegeddon = false;

    private void Start()
    {
        // Store a deep clone of this object as a "save point"
        savedTemplate = Instantiate(gameObject, transform.position, transform.rotation);
        savedTemplate.SetActive(false);
        foreach(Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;
        savedTemplate.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnApplicationQuit() => armegeddon = true;

    private void OnDestroy()
    {
        if (enabled && savedTemplate != null && !armegeddon)
        {
            savedTemplate.SetActive(true);
            foreach (Collider c in savedTemplate.GetComponentsInChildren<Collider>()) c.enabled = true;
            foreach (MonoBehaviour m in savedTemplate.GetComponentsInChildren<MonoBehaviour>()) m.enabled = true;
            savedTemplate.layer = LayerMask.NameToLayer("Default");
        }
    }
}