using UnityEngine;

public class ChirpInputPrinter : MonoBehaviour
{
    [SerializeField] private IChirpGeneratorInput ChirpInput;
    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IChirpGeneratorInput>(this, "ChirpInput");
    }
    private void Update()
    {
        ChirpGeneratorInput input = ChirpInput.NextChirpInput();
        Debug.Log($"ChirpInputPrinter: Generated Chirp Input: {input}");
    }
}