using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider))]
public class LevelEnd : MonoBehaviour
{
    [Tooltip("UIDocument holding LevelEnd.uxml. Falls back to one on this GameObject.")]
    public UIDocument ui;

    [Tooltip("Scene to load. Leave empty to use the next scene in the build list.")]
    public string nextSceneName;

    [Tooltip("Loaded by the menu button, and after the last level.")]
    public string mainMenuSceneName = "MainMenu";

    VisualElement panel;
    bool finished;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (ui == null)
            ui = GetComponent<UIDocument>();

        if (ui == null)
        {
            Debug.LogWarning("LevelEnd has no UIDocument, so no end panel will show.", this);
            return;
        }

        var root = ui.rootVisualElement;
        panel = root.Q<VisualElement>("end-panel");

        var nextButton = root.Q<Button>("next-button");
        if (nextButton != null)
        {
            if (!HasNextScene())
                nextButton.text = "Back to Menu";

            nextButton.clicked += LoadNext;
        }

        var menuButton = root.Q<Button>("menu-button");
        if (menuButton != null)
            menuButton.clicked += LoadMainMenu;

        ShowPanel(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished)
            return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        finished = true;

        player.ChangePlayerState(false);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        ShowPanel(true);
    }

    void ShowPanel(bool visible)
    {
        if (panel != null)
            panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    bool HasNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            return true;

        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }

    void LoadNext()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int next = SceneManager.GetActiveScene().buildIndex + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            LoadMainMenu();
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
