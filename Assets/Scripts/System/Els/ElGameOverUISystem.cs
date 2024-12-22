using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElGameOverUISystem : ElDependency
{
    private ElPauseUISystem _pauseUISystem;
    private ElTimerUISystem _timerUISystem;
    
    private Transform _gameOverUI;

    private TextMeshProUGUI _titleText;

    private Button _restartLevelButton;
    private Button _restartRoomButton;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }
        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }

        _gameOverUI = GameObject.FindWithTag("GameOver").transform;

        TextMeshProUGUI[] texts = _gameOverUI.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("PlayerCapturedText"))
            {
                _titleText = text;
                break;
            }
        }
        
        Button[] buttons = _gameOverUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.CompareTag("RestartLevel"))
            {
                _restartLevelButton = button;
                _restartLevelButton.onClick.AddListener(() =>
                {
                    Creator.playerSystemSo.roomNumberSaved = 0;
        
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else if (button.CompareTag("RestartRoom"))
            {
                _restartRoomButton = button;
                _restartRoomButton.onClick.AddListener(() =>
                {
                    Creator.playerSystemSo.startingPiece = Piece.Queen;

                    float amountToRemove = Creator.timerSo.currentTime / Creator.timerSo.playerRespawnDivideCost;
                    Creator.timerSo.currentTime -= amountToRemove;
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

    public void Show(string message, bool showRestartRoom)
    {
        _restartRoomButton.gameObject.SetActive(showRestartRoom);
        _restartLevelButton.gameObject.SetActive(Creator.timerSo.currentTime > 0);
        _titleText.text = message;
        
        _pauseUISystem.Hide();
        _timerUISystem.HideTimerChangeAmount();
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
