using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Scene the Play button loads. Must be in File > Build Profiles > Scene List.")]
    public string playSceneName = "Level01";

    void OnEnable()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        var root = GetComponent<UIDocument>().rootVisualElement;

        var playButton = root.Q<Button>("play-button");
        if (playButton != null)
            playButton.clicked += Play;

        var quitButton = root.Q<Button>("quit-button");
        if (quitButton != null)
            quitButton.clicked += Quit;
    }

    void Play()
    {
        SceneManager.LoadScene(playSceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
