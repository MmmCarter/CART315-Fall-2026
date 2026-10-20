using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuRoot;
    public GameObject homePanel;
    public GameObject instructionsPanel;

    [Header("Game References")]
    public Snake snake;

    private bool gameStarted;

    private void Start()
    {
        gameStarted = false;

        mainMenuRoot.SetActive(true);
        homePanel.SetActive(true);
        instructionsPanel.SetActive(false);

        // Stop move,but buttons still can be clicked
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;

        if (snake != null)
        {
            snake.ResetState();
        }

        mainMenuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowInstructions()
    {
        homePanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void BackToHome()
    {
        instructionsPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
