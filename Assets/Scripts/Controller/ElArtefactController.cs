using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElArtefactController : Controller
{
    private ElArtefactsUISystem _artefactsUISystem;
    private ElTimerUISystem _timerUISystem;
    
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
        if(addToPlayerArtefactsList)
            Creator.playerSystemSo.artefacts.Add(type);
        _type = type;
    }

    public void SetNotInUse()
    {
        Creator.playerSystemSo.artefacts.Remove(_type);
        switch (_type)
        {
            case ArtefactTypes.EnemyLineOfSight:
                Creator.playerSystemSo.lineOfSightsChosen.Remove(_lineOfSightType);
                break;
        }
        _image.sprite = default;
        _type = ArtefactTypes.None;
    }

    public bool GetInUse()
    {
        return _image.sprite != default;
    }

    public void ShowSellButton()
    {
        _sellButton.gameObject.SetActive(true);
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
