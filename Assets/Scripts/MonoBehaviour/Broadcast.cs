using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UpdatePieceTransform))]
public class Broadcast : MonoBehaviour
{
    [SerializeField] private float baseFreq = 261.626f;
    [SerializeField] private Vector3Int distances;
    [SerializeField] private int read = 0;
    [SerializeField] private int bufferSubunitRatio = 1;

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private int score = 0;

    public ToneUtil toneUtil;
    public Raycaster[] raycasters;
    public UpdatePieceTransform updater;
    public Quaternion rotation = Quaternion.identity;
    public volatile bool requestingUpdate = false;

    private void Awake(){
        toneUtil = new ToneUtil(bufferSubunitRatio, Chord.SameNoteOver3Octaves, Chord.Silence);

        raycasters = GetComponentsInChildren<Raycaster>();
        if (raycasters.Length != 3) throw new InvalidOperationException("Invalid number of stacks found. Requires 3.");

        updater = GetComponent<UpdatePieceTransform>();

        if (statusBar == null) statusBar = GameObject.Find("Text (TMP)");
        if (statusBar == null) Debug.LogWarning("Text (TMP) object not found in scene!");
        else statusText = statusText.GetComponent<TextMeshProUGUI>();
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
}
