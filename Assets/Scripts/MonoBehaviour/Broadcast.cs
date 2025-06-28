using System;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UpdatePieceTransform))]
public class Broadcast : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // AUDIO SYSTEM FIELDS
    // -----------------------------------------------------------------------------
    // Controls tone generation and playback buffering
    [SerializeField] private float baseFreq = 261.626f;               // Base frequency (C4)
    [SerializeField] private int bufferSubunitRatio = 1;              // Buffer ratio for toneUtil
    [SerializeField] private int read = 0;                            // Audio read pointer

    [SerializeField] private int gameMode = 0;                        // Editor handle to configure the toneUtil
    private ToneUtil toneUtil;                                        // Active tone generator instance

    // -----------------------------------------------------------------------------
    // MIC CONFIGURATION
    // -----------------------------------------------------------------------------
    private NativeArray<float>[] leftChannel;
    private NativeArray<float>[] rightChannel;
    private volatile bool validArrayIndex = false;
    [SerializeField] float2 earDistance = new(0.5f,0);
    [SerializeField] private int sampleRate;
    [SerializeField] private float centreAttenuationFactor = 1;
    [SerializeField] private float tiltSensitivity = 0.01f;
    private JobHandle mainBufferRefreshHandle = default;
    private JobHandle binauralBufferRefreshHandle = default;
    [SerializeField] private SpatializationParams sparams;
    private NativeReference<SpatializationParams> sparamsRef;
    public int counters = 0;

    // -----------------------------------------------------------------------------
    // GAMEPLAY STATE FIELDS
    // -----------------------------------------------------------------------------
    private int score = 0;                                            // Player score
    private Quaternion rotation = Quaternion.identity;                // Current piece rotation
    private volatile bool requestingUpdate = false;                   // Triggers stack + audio refresh
    [SerializeField] private GameObject inputManager;
    private TrackingInputInterface inputs;
    // -----------------------------------------------------------------------------
    // STACK + RAYCASTING FIELDS
    // -----------------------------------------------------------------------------
    [SerializeField] private Vector3Int distances;                    // Cached downward raycast distances

    private Raycaster[] raycasters;                                   // Raycasters for 3 stacks
    private UpdatePieceTransform updater;                             // Handles stack reset logic

    // -----------------------------------------------------------------------------
    // UI + SCENE INTEGRATION FIELDS
    // -----------------------------------------------------------------------------
    [SerializeField] private TextMeshProUGUI statusText;              // Score display text UI


    private void Awake() {
        toneUtil = gameMode switch
        {
            0 => new SequentialSingleTone(bufferSubunitRatio, Chord.ExtendedConsonantHarmonics, Chord.Silence),
            1 => new DisjointNoteTermination(bufferSubunitRatio, Chord.SameNoteOver3Octaves, Chord.Silence),
            _ => new DisjointNoteTermination(bufferSubunitRatio, Chord.Silence, Chord.Silence),
        };
        raycasters = GetComponentsInChildren<Raycaster>();
        if (raycasters.Length != 3) throw new InvalidOperationException("Invalid number of stacks found. Requires 3.");
        Array.Sort(raycasters, (a, b) => a.sortIndex.CompareTo(b.sortIndex));

        updater = GetComponent<UpdatePieceTransform>();

        if (inputManager == null) inputManager = GameObject.Find("TrackingInputManager");
        if (inputManager == null) throw new MissingReferenceException("Cannot find required tracking input manager");
        inputs = inputManager.GetComponent<TrackingInputInterface>();

        leftChannel = new NativeArray<float>[2]
        {
            new(toneUtil.MainBuffer, Allocator.Persistent),
            new(toneUtil.MainBuffer, Allocator.Persistent)
        };

        rightChannel = new NativeArray<float>[2]
        {
            new(toneUtil.MainBuffer, Allocator.Persistent),
            new(toneUtil.MainBuffer, Allocator.Persistent)
        };

        sparamsRef = new(sparams, Allocator.Persistent);
        sampleRate = toneUtil.sampleRate;

        if (statusText == null)
        {
            GameObject statusBar = GameObject.Find("Text (TMP)");
            if (statusBar == null) Debug.LogWarning("Text (TMP) object not found in scene!");
            else statusText = statusBar.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (requestingUpdate)
        {
            requestingUpdate = false;
            read = 0;
            distances = new Vector3Int(
                raycasters[0].GetDownwardRaycastDistance(rotation),
                raycasters[1].GetDownwardRaycastDistance(rotation),
                raycasters[2].GetDownwardRaycastDistance(rotation)
            );

            if(distances == Vector3Int.zero)
            {
                statusText.text = $"Score: {++score}";
                updater.ResetStack();
                distances = new Vector3Int(
                    raycasters[0].GetDownwardRaycastDistance(rotation),
                    raycasters[1].GetDownwardRaycastDistance(rotation),
                    raycasters[2].GetDownwardRaycastDistance(rotation)
                );
            }

            bool isMajor = distances.x >= 0f && distances.y >= 0f && distances.z >= 0f;
            if (isMajor)
            {
                int minimum = Mathf.Min(distances.x, distances.y, distances.z);
                distances -= new Vector3Int(minimum, minimum, minimum);
            }
            
            Vector3Int silenceDuration = isMajor ? distances : Vector3Int.zero;
            binauralBufferRefreshHandle.Complete();
            mainBufferRefreshHandle.Complete();
            mainBufferRefreshHandle = toneUtil.RefreshBuffer(baseFreq, isMajor, silenceDuration);
        }
        if (binauralBufferRefreshHandle.IsCompleted)
        {
            binauralBufferRefreshHandle.Complete();
            int writableIndex = validArrayIndex? 1 : 0;
            validArrayIndex = !validArrayIndex;
            leftChannel[writableIndex].AsSpan().Clear();
            rightChannel[writableIndex].AsSpan().Clear();
            sparams = sparamsRef.Value;
            binauralBufferRefreshHandle = new SpatializeAddJob
            {
                input = toneUtil.MainBuffer,
                shiftL = 0,
                shiftR = 0,
                gainL = 1,
                gainR = 1,
                centerGain = 1,
                outputLeft = leftChannel[writableIndex],
                outputRight = rightChannel[writableIndex],
            }.Schedule(toneUtil.MainBuffer.Length, 64,
                new SpatializeAddJob
            {
                input = toneUtil.WhiteBuffer,
                shiftL = sparamsRef.Value.shiftL,
                shiftR = sparamsRef.Value.shiftR,
                gainL = sparamsRef.Value.gainL,
                gainR = sparamsRef.Value.gainR,
                centerGain = sparamsRef.Value.centerGain,
                outputLeft = leftChannel[writableIndex],
                outputRight = rightChannel[writableIndex],
            }.Schedule(toneUtil.WhiteBuffer.Length, 64, JobHandle.CombineDependencies(
                new CreateSpatialisationParamsJob
                {
                    rightEar = earDistance,
                    source = new(
                        math.tanh(inputs.ClockwiseMoment * tiltSensitivity),
                        0.0f
                    ),
                    speedOfSound = 343.0f,
                    sampleRate = toneUtil.sampleRate,
                    centreAttenuationFactor = centreAttenuationFactor,
                    Params = sparamsRef
                }.Schedule(),
                mainBufferRefreshHandle
                )
            ));
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {

            float sampleL = leftChannel[validArrayIndex? 1 : 0][read];
            float sampleR = rightChannel[validArrayIndex? 1 : 0][read];
            switch (channels)
            {
                case 1:
                    // Mono: average of both channels
                    data[i] = 0.5f * (sampleL + sampleR);
                    break;

                case 2:
                    // Stereo: left and right
                    data[i] = sampleL;
                    data[i + 1] = sampleR;
                    break;

                default:
                    // Multichannel: copy left into first, right into second, silence for others
                    data[i] = sampleL;
                    data[i + 1] = sampleR;
                    for (int c = 2; c < channels; c++) data[i + c] = 0f;
                    break;
            }

            read++;

            if (read >= leftChannel[validArrayIndex ? 1 : 0].Length || read >= rightChannel[validArrayIndex ? 1 : 0].Length)
            {
                read %= math.min(leftChannel[validArrayIndex ? 1 : 0].Length, rightChannel[validArrayIndex ? 1 : 0].Length);
                requestingUpdate = true;
            }
        }
    }

    private void OnDestroy()
    {
        binauralBufferRefreshHandle.Complete();
        mainBufferRefreshHandle.Complete();
        toneUtil.Dispose();
        foreach(NativeArray<float> arr in leftChannel) arr.Dispose();
        foreach(NativeArray<float> arr in rightChannel) arr.Dispose();
        sparamsRef.Dispose();
    }
}
