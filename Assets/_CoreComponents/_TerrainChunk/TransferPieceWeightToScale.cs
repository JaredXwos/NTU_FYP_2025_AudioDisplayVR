using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TerrainChunk))]
[RequireComponent(typeof(ScaleBalance))]
public class TransferPieceWeightToScale : MonoBehaviour, IHas<Handler<FitEvent,FitEventPayload>>
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

    Handler<FitEvent, FitEventPayload> Handler;
    Handler<FitEvent, FitEventPayload> IHas<Handler<FitEvent, FitEventPayload>>.Handler => Handler;
}