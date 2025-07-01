using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public abstract class BaseInputInterface : MonoBehaviour
{
    [Header("Interface Output Data")]
    // -----------------------------------------------------------------------------
    [SerializeField, ReadOnly] private float _clockwiseMoment = 0.0f;
    [SerializeField, ReadOnly] private int _pieceOrientation = 0;
    [SerializeField, ReadOnly] private Vector3 _piecePosition = Vector3.zero;

    protected CancellationTokenSource tokenSource;  // This is to send the suicide instruction
    protected CancellationToken token;              // This is to receive the suicide instruction
    protected readonly object inputLock = new();
    private readonly object outputLock = new();

    public float ClockwiseMoment
    {
        get { lock (outputLock) return _clockwiseMoment; }
        protected set { lock (outputLock) _clockwiseMoment = value; }
    }

    public int PieceOrientation
    {
        get { lock (outputLock) return _pieceOrientation; }
        protected set { lock (outputLock) _pieceOrientation = value; }
    }

    public Vector3 PiecePosition
    {
        get { lock (outputLock) return _piecePosition; }
        protected set { lock (outputLock) _piecePosition = value; }
    }

    #region MonoBehavior
    protected virtual void Awake()
    {
        tokenSource = new();
        token = tokenSource.Token;
        Task.Run(BackgroundUpdate);
    }

    protected virtual void OnDestroy() => tokenSource.Cancel();
    #endregion

    protected abstract void BackgroundUpdate();
}