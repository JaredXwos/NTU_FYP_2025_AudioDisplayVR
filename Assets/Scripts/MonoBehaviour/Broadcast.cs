using System;
using TMPro;
using UnityEngine;

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
    // GAMEPLAY STATE FIELDS
    // -----------------------------------------------------------------------------
    private int score = 0;                                            // Player score
    private Quaternion rotation = Quaternion.identity;                // Current piece rotation
    private volatile bool requestingUpdate = false;                   // Triggers stack + audio refresh

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


    private void Awake(){
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
            toneUtil.RefreshBuffer(baseFreq, isMajor, silenceDuration);
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = toneUtil.isSafe? toneUtil.MainBuffer[read++] : 0f;

            for (int c = 0; c < channels; c++)
                data[i + c] = sample;

            if (read >= toneUtil.bufferSubunitSize * 5)
            {
                read %= toneUtil.bufferSubunitSize * 5;
                requestingUpdate = true;
            }
        }
    }

    private void OnDestroy() => toneUtil.Dispose();
}
