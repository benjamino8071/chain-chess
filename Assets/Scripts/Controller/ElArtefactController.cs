using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElArtefactController : Controller
{
    private ElArtefactsUISystem _artefactsUISystem;
    private ElTimerUISystem _timerUISystem;
    private ElXPBarUISystem _xpBarUISystem;
    private ElPlayerSystem _playerSystem;
    
    private Button _guiButton;
    private Image _image;
    private Button _sellButton;
    private TextMeshProUGUI _sellButtonText;

    private ArtefactTypes _type;
    private Piece _lineOfSightType = Piece.NotChosen;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();
        _timerUISystem = elCreator.GetDependency<ElTimerUISystem>();
        _xpBarUISystem = elCreator.GetDependency<ElXPBarUISystem>();
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
    }

    public override void GameEarlyUpdate(float dt)
    {
        if (_sellButtonText.gameObject.activeSelf && _playerSystem.GetState() != ElPlayerSystem.States.Idle)
        {
            HideSellButton();
        }
    }

    public void SetButtonInstance(Button button, Button sellButton)
    {
        button.onClick.AddListener(() =>
        {
            if (GetInUse())
            {
                bool sellButtonShowing = _sellButton.gameObject.activeSelf;
                _artefactsUISystem.HideAllSellButtons();
                if (!sellButtonShowing)
                {
                    ShowSellButton();
                }
            }
        });

        _image = button.GetComponentInChildren<Image>();

        _sellButton = sellButton;

        _sellButtonText = _sellButton.GetComponentInChildren<TextMeshProUGUI>();
        
        _sellButton.onClick.AddListener(() =>
        {
            HideSellButton();
            //TODO: Give the player xp for selling the artefact

            float xpGain = Creator.upgradeSo.sellArtefactXpGain[_type];
            if (_type == ArtefactTypes.EnemyLineOfSight)
            {
                xpGain *= Creator.upgradeSo.pieceValues[_lineOfSightType];
            }
            _xpBarUISystem.IncreaseProgressBarNoMultiplier(xpGain);
            
            SetNotInUse();
        });
        HideSellButton();
        
        _guiButton = button;
        
        SetNotInUse();
    }
    
    public void SetInUse(ArtefactTypes type, bool addToPlayerArtefactsList, Piece lineOfSight = Piece.NotChosen)
    {
        switch (type)
        {
            case ArtefactTypes.EnemyLineOfSight:
                _lineOfSightType = lineOfSight;
                switch (lineOfSight)
                {
                    case Piece.Pawn:
                        _image.sprite = Creator.shopSo.pawnLosSprite;
                        break;
                    case Piece.Knight:
                        _image.sprite = Creator.shopSo.knightLosSprite;
                        break;
                    case Piece.Bishop:
                        _image.sprite = Creator.shopSo.bishopLosSprite;
                        break;
                    case Piece.Rook:
                        _image.sprite = Creator.shopSo.rookLosSprite;
                        break;
                    case Piece.Queen:
                        _image.sprite = Creator.shopSo.queenLosSprite;
                        break;
                    case Piece.King:
                        _image.sprite = Creator.shopSo.kingLosSprite;
                        break;
                }
                break;
            case ArtefactTypes.CaptureKingClearRoom:
                _image.sprite = Creator.shopSo.captureKingClearRoomSprite;
                break;
            case ArtefactTypes.UseCapturedPieceStraightAway:
                _image.sprite = Creator.shopSo.useCapturedPieceStraightAwaySprite;
                break;
            case ArtefactTypes.ConCaptureAttackingEnemy:
                _image.sprite = Creator.shopSo.destroyChainStayAliveSprite;
                break;
        }
        _image.color = new Color(1, 1, 1, 1);

        if (addToPlayerArtefactsList)
        {
            Creator.upgradeSo.artefactsChosen.Add(type);
            if (lineOfSight != Piece.NotChosen)
            {
                Creator.upgradeSo.lineOfSightsChosen.Add(lineOfSight);
            }
        }
        _type = type;
        
        float xpGain = Creator.upgradeSo.sellArtefactXpGain[_type];

        if (_type == ArtefactTypes.EnemyLineOfSight)
        {
            xpGain *= Creator.upgradeSo.pieceValues[_lineOfSightType];
        }

        _sellButtonText.text = $"Sell: {xpGain:0}xp";
    }

    public void SetNotInUse()
    {
        Creator.upgradeSo.artefactsChosen.Remove(_type);
        switch (_type)
        {
            case ArtefactTypes.EnemyLineOfSight:
                Creator.upgradeSo.lineOfSightsChosen.Remove(_lineOfSightType);
                break;
        }
        _image.color = new Color(1, 1, 1, 0.003f);
        _image.sprite = default;
        _type = ArtefactTypes.None;
    }

    public bool GetInUse()
    {
        return _image.sprite != default;
    }

    public void ShowSellButton()
    {
        //_sellButton.gameObject.SetActive(_playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void HideSellButton()
    {
        _sellButton.gameObject.SetActive(false);
    }

    public ArtefactTypes GetArtefactType()
    {
        return _type;
    }
}
