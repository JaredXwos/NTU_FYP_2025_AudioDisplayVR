using System;
using System.Linq;
using UnityEngine;

public class ModifiedGroundSonar: GroundSonar
{
    protected override void Update()
    {
        Vector3 downward = transform.TransformDirection(Vector3.down);

        Vector3[] startPoint = ComponentTransforms
            .Select(p => new Vector3(p.position.x, p.position.y - p.localScale.y / 2, p.position.z) - downward * 0.1f)
            .OrderBy(x => x.x)
            .ThenBy(x => x.z)
            .ToArray();
        GameObject collided = null;
        for (int i = 0; i < startPoint.Length; i++)
            if (Physics.Raycast(startPoint[i], downward, out RaycastHit hit, 10f) && hit.transform.root.gameObject != gameObject)
            {
                groundClearance[i] = Mathf.FloorToInt(hit.distance);
                collided = hit.transform.root.gameObject;
            }
            else
            {
                collided = null;
                Array.Fill(groundClearance, -1);
                break;
            }

        _groundClearance.Value = (int[])groundClearance.Clone();

        if (broadcastFitEvent && collided != null && collided.transform.root.gameObject.GetComponent<CoreComponent>() is TerrainChunk && groundClearance.All(h => h == 0) && !isCurrentlyFit)
            Invoke(new FitEventPayload(Parent, collided, true));

        isCurrentlyFit = collided != null && groundClearance.All(h => h == 0);
    }
}