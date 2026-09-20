using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelRestart : MonoBehaviour
{
    public Key restartKey = Key.R;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[restartKey].wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
