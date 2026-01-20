using UnityEngine;

public class QuitOnEsc : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Application.Quit() called");
        }
    }
}