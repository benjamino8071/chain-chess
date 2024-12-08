using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevPauseUISystem : LevDependency
{
    private LevPlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        if (levCreator.NewTryGetDependency(out LevPlayerSystem levPlayerSystem))
        {
            _playerSystem = levPlayerSystem;
        }
        
        _pauseGUI = GameObject.FindWithTag("Pause").transform;
        
        _pauseButton = GameObject.FindWithTag("PauseButton").GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetState(LevPlayerSystem.States.WaitingForTurn);
            }
            else
            {
                Hide();
                _playerSystem.SetState(LevPlayerSystem.States.Idle);
            }
        });

        Button[] buttons = _pauseGUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.CompareTag("ResetButton"))
            {
                button.onClick.AddListener(() =>
                {
                    Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        
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

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == LevPlayerSystem.States.Idle);
    }

    public void Show()
    {
        _pauseGUI.gameObject.SetActive(true);

    }

    public void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);
    }
}
