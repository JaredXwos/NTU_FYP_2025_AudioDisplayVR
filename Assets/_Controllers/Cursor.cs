using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class GrabEventHandler : Handler<CoreComponent>
{
    public GrabEventHandler(Action<CoreComponent> handler) : base(handler)
    {
    }
}

public class Cursor : Dispatch
{
    [Header("Grabbing")]
    [SerializeField] private float grabRadius;
    [SerializeField, ReadOnly] private int grabCount = 0;
    [SerializeField] private bool grabEnabled;

    [Header("Input Interface")]
    [SerializeField] private InputInterface input;

    private readonly Dictionary<IGrabbable, (int, Vector3)> grabbed = new();

    protected override Type HandlerType { get; set; }
    protected override Type PayloadType { get; set; }

    protected override void Awake()
    {
        HandlerType = typeof(GrabEventHandler);
        PayloadType = typeof(CoreComponent);
        base.Awake();

        InputInterface[] inputs = GetComponents<InputInterface>();
        input ??= inputs.FirstOrDefault(ii => ii.enabled) ?? inputs.First();
        if (!input.enabled) input.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Update()
    {
        transform.position = new Vector3(
            Mathf.RoundToInt(input.PiecePosition.x),
            Mathf.RoundToInt(input.PiecePosition.y),
            Mathf.RoundToInt(input.PiecePosition.z)
        );
        if (grabEnabled)
        {
            if (input.IsGrabbing)
            {
                foreach (Collider hit in Physics.OverlapSphere(transform.position, grabRadius))
                {
                    var grabbable = hit.GetComponentInParent<IGrabbable>();
                    if (grabbable == null) continue;

                    if (!grabbed.ContainsKey(grabbable))
                    {
                        grabbed[grabbable] = (grabbable.Orientation - input.PieceOrientation, grabbable.Position - transform.position);
                        Invoke(hit.transform.root.gameObject.GetComponent<CoreComponent>());
                    }
                        
                }
                grabCount = grabbed.Count;
                foreach(var(grabbable, (rotationDifference, positionDifference)) in grabbed)
                    grabbable.SetTransform(transform.position + positionDifference, input.PieceOrientation + rotationDifference);
            }
            else
            {
                grabbed.Clear();
                input.PieceOrientation = 0;
            }
        }
    }

}