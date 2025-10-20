using UnityEngine;

public class CommandInputPrinter : MonoBehaviour
{
    [Header("Input Source (must implement IDroneCommandInput)")]
    private IDroneCommandInput commandInput;

    void Awake()
    {
        // Validate the input source
        Check.PropertyEnabledElseAssign<IDroneCommandInput>(this, "commandInput");
    }

    void Update()
    {
        if (commandInput == null || !commandInput.IsActive())
            return;

        DroneCommand cmd = commandInput.GetCommand();
        Debug.Log($"[DroneCommand] Roll={cmd.Roll:F2}, Pitch={cmd.Pitch:F2}, Yaw={cmd.Yaw:F2}, Altitude={cmd.Altitude:F2}");
    }
}