using TMPro;
using UnityEngine;

public class NonAudioMonitor : MonoBehaviour, IHas<EventHandler<GrabEvent, IPParentCoreComponent>>
{
    protected float Weight;
    protected float Tilt;
    [SerializeField] protected ScaleBalance_Deprecated ScaleBalance;
    [SerializeField] protected TextMeshProUGUI TextMesh;

    private void Awake()
    {
        Handler = new(
            p =>
            {
                if(!p.Parent.gameObject.TryGetComponent(out ILoad weight)) return;
                Weight = weight.Force.magnitude;
            },
            "NonAudioMonitor"
        );
        Check.PropertyEnabledElseAssign<ScaleBalance_Deprecated>(this, "ScaleBalance");
        Check.PropertyEnabledElseAssign<TextMeshProUGUI>(this, "TextMesh");
    }
    protected virtual void FixedUpdate()
    {
        Tilt = ScaleBalance.Orientation.magnitude;
        TextMesh.text = $"Weight: {Weight}\nTilt: {Tilt}";
    }
    protected EventHandler<GrabEvent, IPParentCoreComponent> Handler;
    EventHandler<GrabEvent, IPParentCoreComponent> IHas<EventHandler<GrabEvent, IPParentCoreComponent>>.Handler => Handler;
}