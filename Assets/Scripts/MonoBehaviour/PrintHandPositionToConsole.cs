using UnityEngine;
using Leap;

public class PrintHandPositionToConsole : MonoBehaviour
{
    [Tooltip("Optional: Assign the CapsuleHand manually. If left null, it will auto-detect.")]
    [SerializeField] private HandModelBase capsuleHand;

    private void Start()
    {
        if (capsuleHand == null)
        {
            Debug.LogWarning("No CapsuleHand assigned — attempting to auto-detect...");
            capsuleHand = FindTrackedCapsuleHand();
            if (capsuleHand == null) Debug.LogError("No active CapsuleHand found in scene!");
            else Debug.Log("A hand has been automatically assigned.");
        }
    }

    private void Update()
    {
        if (capsuleHand != null && capsuleHand.IsTracked)
        {
            var hand = capsuleHand.GetLeapHand();
            Vector3 palm = hand.PalmPosition;
            Debug.Log($"{(hand.IsLeft ? "Left" : "Right")} hand palm at: {palm}");
        }
    }

    private HandModelBase FindTrackedCapsuleHand()
    {
        HandModelBase[] allHands = FindObjectsByType<HandModelBase>(FindObjectsSortMode.None);
        foreach (var hand in allHands) return hand;
        return null;
    }
}