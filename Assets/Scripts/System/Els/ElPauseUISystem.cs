using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElPauseUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private GameObject _upgradeNotificationImage;

    private Button _pauseButton;

    private PauseUITextInfo _pauseUITextInfo;

    private bool _canShow;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();

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
                Hide();
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
            }
        });
        
        UpdateCaptureBonusText();
        UpdateBaseMultiplierText();
        HideUpgradeNotificationImage();
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void Show()
    {
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

    public void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);
    }
    
    public void UpdateCaptureBonusText()
    {
        bool hasUpgraded = Creator.upgradeSo.capturePieceXPGain[Piece.Pawn] > 1;

        if (hasUpgraded)
        {
            _pauseUITextInfo.pawnCapValueText.text = $"Pawn: +1 (+{Creator.upgradeSo.capturePieceXPGain[Piece.Pawn] - 1:0.##})xp";
            _pauseUITextInfo.knightCapValueText.text = $"Knight: +3 (+{Creator.upgradeSo.capturePieceXPGain[Piece.Knight] - 3:0.##})xp";
            _pauseUITextInfo.bishopCapValueText.text = $"Bishop: +3 (+{Creator.upgradeSo.capturePieceXPGain[Piece.Bishop] - 3:0.##})xp";
            _pauseUITextInfo.rookCapValueText.text = $"Rook: +5 (+{Creator.upgradeSo.capturePieceXPGain[Piece.Rook] - 5:0.##})xp";
            _pauseUITextInfo.queenCapValueText.text = $"Queen: +9 (+{Creator.upgradeSo.capturePieceXPGain[Piece.Queen] - 9:0.##})xp";
            _pauseUITextInfo.kingCapValueText.text = $"King: +9 (+{Creator.upgradeSo.capturePieceXPGain[Piece.King] - 9:0.##})xp";
        }
        else
        {
            _pauseUITextInfo.pawnCapValueText.text = $"Pawn: +{Creator.upgradeSo.capturePieceXPGain[Piece.Pawn]:0.##}xp";
            _pauseUITextInfo.knightCapValueText.text = $"Knight: +{Creator.upgradeSo.capturePieceXPGain[Piece.Knight]:0.##}xp";
            _pauseUITextInfo.bishopCapValueText.text = $"Bishop: +{Creator.upgradeSo.capturePieceXPGain[Piece.Bishop]:0.##}xp";
            _pauseUITextInfo.rookCapValueText.text = $"Rook: +{Creator.upgradeSo.capturePieceXPGain[Piece.Rook]:0.##}xp";
            _pauseUITextInfo.queenCapValueText.text = $"Queen: +{Creator.upgradeSo.capturePieceXPGain[Piece.Queen]:0.##}xp";
            _pauseUITextInfo.kingCapValueText.text = $"King: +{Creator.upgradeSo.capturePieceXPGain[Piece.King]:0.##}xp";
        }
    }

    public void UpdateBaseMultiplierText()
    {
        if (Creator.upgradeSo.baseMultiplier > 1)
        {
            _pauseUITextInfo.baseMultiplierValueText.text = $"Base Multiplier: ({Creator.upgradeSo.baseMultiplier:0.##}) \u00d7 1\u00d7";
        }
        else
        {
            _pauseUITextInfo.baseMultiplierValueText.text = $"Base Multiplier: 1\u00d7";
        }
        
    }
}
