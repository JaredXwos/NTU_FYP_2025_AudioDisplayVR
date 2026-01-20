using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    int frameCount = 0;
    float timer = 0f;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;      // Turn off VSync
        Application.targetFrameRate = 1000;
    }

    void Update()
    {
        frameCount++;
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            Debug.Log("Update() calls per second: " + frameCount);
            frameCount = 0;
            timer -= 1f;
        }
    }
}