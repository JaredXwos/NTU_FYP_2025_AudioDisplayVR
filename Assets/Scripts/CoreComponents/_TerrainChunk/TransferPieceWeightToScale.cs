using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TerrainChunk))]
[RequireComponent(typeof(ScaleBalance))]
public class TransferPieceWeightToScale : MonoBehaviour, IHas<EventHandler<FitEvent,FitEventPayload>>
{
    [SerializeField] TerrainChunk chunk;
    [SerializeField] ScaleBalance balance;
    [SerializeField] GameObject[] body;

    private void Awake()
    {
        chunk ??= GetComponent<TerrainChunk>();
        balance ??= GetComponent<ScaleBalance>();
        body = transform.GetComponentsInChildren<Transform>()
                                       .Select(t => t.gameObject)
                                       .ToArray();
        Handler = new(payload =>
        {
            if (body.Contains(payload.Collidee))
            {
                ILoad[] weights = payload.Parent.gameObject.GetComponents<ILoad>();
                balance.RegisterWeight(weights);
            }
        }, $"Transfer Piece Weight to Scale on {gameObject.name}");
    }

    EventHandler<FitEvent, FitEventPayload> Handler;
    EventHandler<FitEvent, FitEventPayload> IHas<EventHandler<FitEvent, FitEventPayload>>.Handler => Handler;
}