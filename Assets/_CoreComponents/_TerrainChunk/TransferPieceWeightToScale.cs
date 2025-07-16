using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TerrainChunk))]
[RequireComponent(typeof(ScaleBalance))]
public class TransferPieceWeightToScale : MonoBehaviour, IHas<FitEventHandler<Piece>>
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
    }

    FitEventHandler<Piece> IHas<FitEventHandler<Piece>>.Handler => new(
        ((Piece piece, GameObject gameObject) payload) =>
        {
            if (body.Contains(payload.gameObject))
            {
                ILoad[] weights = payload.piece.gameObject.GetComponents<ILoad>();
                balance.RegisterWeight(weights);
            }
        });
}