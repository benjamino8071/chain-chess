using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElGameOverUISystem : ElDependency
{
    private Transform _gameOverUI;

    private TextMeshProUGUI _titleText;

    private Button _restartRoomButton;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

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
            if (button.CompareTag("RestartRoom"))
            {
                _restartRoomButton = button;
                _restartRoomButton.onClick.AddListener(() =>
                {
                    Creator.timerSo.currentTime -= Creator.timerSo.contFromRoomPenalty;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else if (button.CompareTag("RestartLevel"))
            {
                button.onClick.AddListener(() =>
                {
                    Creator.playerSystemSo.roomNumberSaved = 0;
                    Creator.timerSo.currentTime = Creator.timerSo.maxTime;
        
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else if (button.CompareTag("Exit"))
            {
                //TODO: Exit to main menu
            }
        }

        Hide();
    }

    public void Show(string message, bool showRestartRoom)
    {
        _restartRoomButton.gameObject.SetActive(showRestartRoom);
        _titleText.text = message;
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
