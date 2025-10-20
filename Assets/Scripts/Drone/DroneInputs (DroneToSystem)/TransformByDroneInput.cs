using UnityEngine;

public class TransforByDroneInput : MonoBehaviour
{
    [SerializeField] private IDroneInputInterface DroneInput;
    private void Awake() => Check.PropertyEnabledElseAssign<IDroneInputInterface>(this, "DroneInput");
    private void Update()
    {
        if (DroneInput == null) return;
        transform.rotation = Quaternion.Euler( (float)DroneInput.Theta * Mathf.Rad2Deg, -(float)DroneInput.Psi * Mathf.Rad2Deg, (float)DroneInput.Phi * Mathf.Rad2Deg);
        transform.position = new Vector3(-(float)DroneInput.Y, (float)DroneInput.Z, (float)DroneInput.X);
    }
}