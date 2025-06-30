using UnityEngine;

public class Lighten : MonoBehaviour
{
    private Renderer[] renderers;
    [SerializeField] private float H = 0;
    [SerializeField] private float S = 0;
    [SerializeField] private float V = 0;
    [SerializeField] public int currentStep = 0;
    [SerializeField] private int steps = 5;
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
            Color.RGBToHSV(renderer.material.color, out float H, out float S, out V);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Renderer renderer in renderers)
            renderer.material.color = Color.HSVToRGB(H, S, V * currentStep/steps);
    }
}
