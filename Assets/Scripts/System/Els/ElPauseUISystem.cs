using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElPauseUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElTimerUISystem _timerUISystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private PauseUITextInfo _pauseUITextInfo;

    private bool _canShow;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (elCreator.TryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (elCreator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        
        _pauseGUI = GameObject.FindWithTag("Pause").transform;
        
        _pauseUITextInfo = _pauseGUI.GetComponentInChildren<PauseUITextInfo>();

        Button[] buttons = _pauseGUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.CompareTag("Exit"))
            {
                button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene("MainMenuScene");
                });
            }
        }
        
        _pauseButton = GameObject.FindWithTag("PauseButton").GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetState(ElPlayerSystem.States.WaitingForTurn);
            }
            else
            {
                Hide();
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
            }
        });
        
        UpdateTextInfo();
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void Show()
    {
        _timerUISystem.StopTimer();
        _pauseGUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);
        _timerUISystem.StartTimer();
    }
    
    public void UpdateTextInfo()
    {
        _pauseUITextInfo.pawnCapValueText.text = $"Pawn: +{Creator.timerSo.capturePieceTimeAdd[Piece.Pawn]:0.##}s";
        _pauseUITextInfo.knightCapValueText.text = $"Knight: +{Creator.timerSo.capturePieceTimeAdd[Piece.Knight]:0.##}s";
        _pauseUITextInfo.bishopCapValueText.text = $"Bishop: +{Creator.timerSo.capturePieceTimeAdd[Piece.Bishop]:0.##}s";
        _pauseUITextInfo.rookCapValueText.text = $"Rook: +{Creator.timerSo.capturePieceTimeAdd[Piece.Rook]:0.##}s";
        _pauseUITextInfo.queenCapValueText.text = $"Queen: +{Creator.timerSo.capturePieceTimeAdd[Piece.Queen]:0.##}s";
        _pauseUITextInfo.kingCapValueText.text = $"King: +{Creator.timerSo.capturePieceTimeAdd[Piece.King]:0.##}s";
        
        float mulMulPer = (Creator.timerSo.timerMultiplierMultiplier * 100) - 100;
        _pauseUITextInfo.pieceCapMulMulText.text = $"Consecutive Capture Multiplier Increase: \u2191{(int)mulMulPer}%";

        float restartRoomPenaltyPer = (1 / Creator.timerSo.playerRespawnDivideCost) * 100;
        _pauseUITextInfo.restartRoomTimePenalty.text = $"Restart Room Time Penalty: -{(int)restartRoomPenaltyPer}% Current Time";
    }
}
