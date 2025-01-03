using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElPauseUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElTimerUISystem _timerUISystem;
    
    private Transform _pauseGUI;

    private GameObject _upgradeNotificationImage;

    private Button _pauseButton;

    private PauseUITextInfo _pauseUITextInfo;

    private bool _canShow;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        _timerUISystem = elCreator.GetDependency<ElTimerUISystem>();

        _pauseGUI = elCreator.GetFirstObjectWithName(AllTagNames.Pause);
        
        _pauseUITextInfo = _pauseGUI.GetComponentInChildren<PauseUITextInfo>();

        Transform exitButtonTf = elCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.Exit);

        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });

        Transform pauseButton = elCreator.GetFirstObjectWithName(AllTagNames.PauseButton);
        _pauseButton = pauseButton.GetComponent<Button>();

        Transform upgradeNotificationImage =
            elCreator.GetChildObjectByName(pauseButton.gameObject, AllTagNames.Notification);
        _upgradeNotificationImage = upgradeNotificationImage.gameObject;
        
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetState(ElPlayerSystem.States.WaitingForTurn);
            }
            else
            {
                Hide(true);
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
            }
        });
        
        UpdateTextInfo();
        HideUpgradeNotificationImage();
        Hide(false);
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void Show()
    {
        //_timerUISystem.StopTimer();
        _pauseGUI.gameObject.SetActive(true);
        HideUpgradeNotificationImage();
    }

    public void ShowUpgradeNotificationImage()
    {
        _upgradeNotificationImage.SetActive(true);
    }

    public void HideUpgradeNotificationImage()
    {
        _upgradeNotificationImage.SetActive(false);
    }

    public void Hide(bool startTimer)
    {
        _pauseGUI.gameObject.SetActive(false);
        // if(startTimer && Creator.playerSystemSo.moveMadeInNewRoom)
        //     _timerUISystem.StartTimer();
    }
    
    public void UpdateTextInfo()
    {
        _pauseUITextInfo.pawnCapValueText.text = $"Pawn: +{1:0.##}xp";
        _pauseUITextInfo.knightCapValueText.text = $"Knight: +{1:0.##}xp";
        _pauseUITextInfo.bishopCapValueText.text = $"Bishop: +{1:0.##}xp";
        _pauseUITextInfo.rookCapValueText.text = $"Rook: +{1:0.##}xp";
        _pauseUITextInfo.queenCapValueText.text = $"Queen: +{1:0.##}xp";
        _pauseUITextInfo.kingCapValueText.text = $"King: +{1:0.##}xp";
        
        _pauseUITextInfo.pieceCapMulMulText.text = $"Consecutive Capture Multiplier Increase: \u2191{(int)1}%";

        _pauseUITextInfo.restartRoomTimePenalty.text = $"Restart Room Penalty: -{(int)1}%";
    }
}
