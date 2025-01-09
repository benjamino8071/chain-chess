using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElGameOverUISystem : ElDependency
{
    private ElPauseUISystem _pauseUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    private ElLivesUISystem _livesUISystem;
    
    private Transform _gameOverUI;

    private TextMeshProUGUI _titleText;

    private Button _restartRoomButton;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _pauseUISystem = elCreator.GetDependency<ElPauseUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();
        _livesUISystem = elCreator.GetDependency<ElLivesUISystem>();

        _gameOverUI = elCreator.GetFirstObjectWithName(AllTagNames.GameOver);

        Transform titleText = elCreator.GetFirstObjectWithName(AllTagNames.PlayerCapturedText);
        _titleText = titleText.GetComponent<TextMeshProUGUI>();
        
        Button[] buttons = _gameOverUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            TagName tagName = button.GetComponent<TagName>();
            
            switch (tagName.tagName)
            {
                case AllTagNames.RestartRoom:
                    _restartRoomButton = button;
                    _restartRoomButton.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    });
                    break;
                case AllTagNames.Exit:
                    button.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("MainMenuScene");
                    });
                    break;
            }
        }

        Hide();
    }

    public void Show(string message, bool showRestartRoom)
    {
        _titleText.text = message;
        _livesUISystem.LoseLife();

        _pauseUISystem.Hide();
        _artefactsUISystem.Hide();
        _restartRoomButton.gameObject.SetActive(showRestartRoom && Creator.livesSo.livesRemaining > 0);
        
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
