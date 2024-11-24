using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUISystem : Dependency
{
    private PlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        if(_creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }

        _pauseGUI = GameObject.FindWithTag("Pause").transform;
        
        _pauseButton = GameObject.FindWithTag("PauseButton").GetComponent<Button>();
        _pauseButton.onClick.AddListener(TogglePause);
        
        _pauseGUI.GetComponentInChildren<Button>().onClick.AddListener(GiveUp);
        
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == PlayerSystem.States.Idle);
    }

    private void GiveUp()
    {
        _creator.playerSystemSo.startingPiece = Piece.NotChosen;
        _creator.playerSystemSo.roomNumberSaved = 0;
        _creator.timerSo.currentTime = _creator.timerSo.maxTime;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TogglePause()
    {
        if (!_pauseGUI.gameObject.activeSelf)
        {
            Show();
            _playerSystem.SetState(PlayerSystem.States.WaitingForTurn);
        }
        else
        {
            Hide();
            _playerSystem.SetState(PlayerSystem.States.Idle);
        }
    }

    private void Show()
    {
        _pauseGUI.gameObject.SetActive(true);

    }

    private void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);

    }
}
