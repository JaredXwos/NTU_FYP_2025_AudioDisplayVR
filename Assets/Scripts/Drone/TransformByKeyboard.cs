using UnityEngine;

/// <summary>
/// Simple keyboard flight/movement controller.
/// WASD = horizontal/forward movement
/// R/F = rise/fall (vertical movement)
/// Q/E = yaw rotation (turn left/right)
/// </summary>
public class TransformByKeyboard : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;     // units per second
    public float riseSpeed = 5f;     // vertical speed
    public float yawSpeed = 90f;     // degrees per second

    void Update()
    {
        // Movement input
        float moveForward = 0f;
        if (Input.GetKey(KeyCode.W)) moveForward += 1f;
        if (Input.GetKey(KeyCode.S)) moveForward -= 1f;

        float moveRight = 0f;
        if (Input.GetKey(KeyCode.D)) moveRight += 1f;
        if (Input.GetKey(KeyCode.A)) moveRight -= 1f;

        float moveUp = 0f;
        if (Input.GetKey(KeyCode.R)) moveUp += 1f;
        if (Input.GetKey(KeyCode.F)) moveUp -= 1f;

        // Combine movement directions (local space)
        Vector3 moveDir = transform.forward * moveForward +
                          transform.right * moveRight +
                          transform.up * moveUp;

        // Normalize diagonal movement
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // Apply translation
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // Yaw rotation (Q/E)
        float yaw = 0f;
        if (Input.GetKey(KeyCode.Q)) yaw -= 1f;
        if (Input.GetKey(KeyCode.E)) yaw += 1f;

        transform.Rotate(Vector3.up, yaw * yawSpeed * Time.deltaTime, Space.Self);
    }
}