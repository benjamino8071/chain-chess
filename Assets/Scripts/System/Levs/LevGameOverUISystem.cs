using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevGameOverUISystem : LevDependency
{
    private LevPauseUISystem _pauseUISystem;
    
    private Transform _gameOverUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        if (Creator.NewTryGetDependency(out LevPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }

        _gameOverUI = GameObject.FindWithTag("GameOver").transform;
        
        Button[] buttons = _gameOverUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.CompareTag("RestartLevel"))
            {
                button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else if (button.CompareTag("Exit"))
            {
                button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene("MainMenuScene");
                });
            }
        }

        Hide();
    }

    public void Show()
    {
        _pauseUISystem.Hide();
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
