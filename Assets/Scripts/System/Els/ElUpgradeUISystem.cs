using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElUpgradeUISystem : ElDependency
{
    private ElXPBarUISystem _xpBarUISystem;
    private ElPlayerSystem _playerSystem;
    private ElPauseUISystem _pauseUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private Transform _upgradeUI;
    
    private MMF_Player _upgradeSelectedPlayer;

    private Button _upgradeOneButton;
    private Image _upgradeOneImage;
    private TextMeshProUGUI _upgradeOneExplanationText;
    private UpgradeTypes _upgradeOneType;
    
    private Button _upgradeTwoButton;
    private Image _upgradeTwoImage;
    private TextMeshProUGUI _upgradeTwoExplanationText;
    private UpgradeTypes _upgradeTwoType;

    private bool _isArtefact;
    private bool _isArtefactUpgradeTwo; //If false, it will be _upgradeOneButton
    private ArtefactTypes _artefactChosen = ArtefactTypes.None;
    private Piece _losPieceChosen = Piece.NotChosen;

    private List<UpgradeTypes> _upgradeTypes = new()
    {
        UpgradeTypes.IncreaseBaseMultiplierAmount,
        UpgradeTypes.IncreasePromotionXPGain,
        UpgradeTypes.IncreaseBaseAmountGained
    };
    //Reduce respawn cost is currently NOT IN USE

    private Dictionary<UpgradeTypes, string> _upgradeTypeExplanationText = new()
    {
        { UpgradeTypes.IncreaseBaseMultiplierAmount, "Multiplier resets at a higher value."},
        { UpgradeTypes.IncreaseBaseAmountGained, "Gain more XP for capturing a piece."},
        { UpgradeTypes.IncreasePromotionXPGain, "Gain more XP after promoting a pawn."}
    };
    
    private List<ArtefactTypes> _artefactTypes = new()
    {
        ArtefactTypes.ConCaptureAttackingEnemy,
        ArtefactTypes.CaptureKingClearRoom,
        ArtefactTypes.EnemyLineOfSight,
        ArtefactTypes.UseCapturedPieceStraightAway
    };

    private Dictionary<ArtefactTypes, string> _artefactTypeExplanationText = new()
    {
        { ArtefactTypes.ConCaptureAttackingEnemy, "When an enemy tries to capture you, capture them instead.\\nDestroyed on use." },
        { ArtefactTypes.CaptureKingClearRoom, "Clear the entire room upon capturing the King." },
        { ArtefactTypes.EnemyLineOfSight, "View the line of sight of the Piece shown." },
        { ArtefactTypes.UseCapturedPieceStraightAway, "When you capture a Piece, it gets added directly after your current Piece." }
    };

    private bool _choosingUpgrade;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        _pauseUISystem = elCreator.GetDependency<ElPauseUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();
        
        _upgradeUI = elCreator.GetFirstObjectWithName(AllTagNames.Upgrade);
        
        Transform guiRight = elCreator.GetFirstObjectWithName(AllTagNames.GUIRight);
        
        Transform upgradeSelectedPlayer =
            elCreator.GetChildObjectByName(guiRight.gameObject, AllTagNames.MmfUpgradeSelectedPlayer);
        _upgradeSelectedPlayer = upgradeSelectedPlayer.GetComponent<MMF_Player>();

        List<Transform> upgradeButtons = elCreator.GetChildObjectsByName(_upgradeUI.gameObject, AllTagNames.UpgradeButton);

        if (upgradeButtons.Count != 2)
        {
            Debug.LogError("There must be only two upgrade buttons!");
        }
        
        //UPGRADE ONE
        _upgradeOneButton = upgradeButtons[0].GetComponent<Button>();
        Transform upgradeOneImage = elCreator.GetChildObjectByName(_upgradeOneButton.gameObject, AllTagNames.Image);
        _upgradeOneImage = upgradeOneImage.GetComponent<Image>();
        
        Transform upgradeOneText = elCreator.GetChildObjectByName(_upgradeOneButton.gameObject, AllTagNames.Text);
        _upgradeOneExplanationText = upgradeOneText.GetComponent<TextMeshProUGUI>();
        //
        
        //UPGRADE TWO
        _upgradeTwoButton = upgradeButtons[1].GetComponent<Button>();
        Transform upgradeTwoImage = elCreator.GetChildObjectByName(_upgradeTwoButton.gameObject, AllTagNames.Image);
        _upgradeTwoImage = upgradeTwoImage.GetComponent<Image>();
        
        Transform upgradeTwoText = elCreator.GetChildObjectByName(_upgradeTwoButton.gameObject, AllTagNames.Text);
        _upgradeTwoExplanationText = upgradeTwoText.GetComponent<TextMeshProUGUI>();
        //
        
        _upgradeOneButton.onClick.AddListener(UpgradeOneButtonPressed);
        _upgradeTwoButton.onClick.AddListener(UpgradeTwoButtonPressed);
        
        Hide();
    }

    private void UpgradeOneButtonPressed()
    {
        if (_isArtefact && !_isArtefactUpgradeTwo)
        {
            if (_artefactsUISystem.TryAddArtefact(_artefactChosen, _losPieceChosen))
            {
                _isArtefact = false;
                _artefactChosen = ArtefactTypes.None;
                _losPieceChosen = Piece.NotChosen;
                Hide();
            }
        }
        else
        {
            ApplyUpgrade(_upgradeOneType);
            _isArtefact = false;
            _artefactChosen = ArtefactTypes.None;
            _losPieceChosen = Piece.NotChosen;
            Hide();
        }
    }

    private void UpgradeTwoButtonPressed()
    {
        if (_isArtefact && _isArtefactUpgradeTwo)
        {
            if (_artefactsUISystem.TryAddArtefact(_artefactChosen, _losPieceChosen))
            {
                _isArtefact = false;
                _artefactChosen = ArtefactTypes.None;
                _losPieceChosen = Piece.NotChosen;
                Hide();
            }
        }
        else
        {
            ApplyUpgrade(_upgradeTwoType);
            _isArtefact = false;
            _artefactChosen = ArtefactTypes.None;
            _losPieceChosen = Piece.NotChosen;
            Hide();
        }
    }
    
    private void SetUpgradeButtonData(UpgradeTypes upgradeType, Image buttonImage, TextMeshProUGUI buttonExplanationText)
    {
        switch (upgradeType)
        {
            case UpgradeTypes.IncreaseBaseMultiplierAmount:
                buttonImage.sprite = Creator.upgradeSo.increaseMultiplierAmountSprite;
                break;
            case UpgradeTypes.IncreasePromotionXPGain:
                buttonImage.sprite = Creator.upgradeSo.increasePromoXpGainSprite;
                break;
            case UpgradeTypes.IncreaseBaseAmountGained:
                buttonImage.sprite = Creator.upgradeSo.increaseBaseAmountGainedSprite;
                break;
        }
        buttonExplanationText.text = _upgradeTypeExplanationText[upgradeType];
    }

    private void SetArtefactButtonData(ArtefactTypes artefactType, Piece losPiece, Image buttonImage,
        TextMeshProUGUI buttonExplanationText)
    {
        switch (artefactType)
        {
            case ArtefactTypes.CaptureKingClearRoom:
                buttonImage.sprite = Creator.upgradeSo.captureKingClearRoomSprite;
                break;
            case ArtefactTypes.ConCaptureAttackingEnemy:
                buttonImage.sprite = Creator.upgradeSo.destroyChainStayAliveSprite;
                break;
            case ArtefactTypes.EnemyLineOfSight:
                switch (losPiece)
                {
                    case Piece.Bishop:
                        buttonImage.sprite = Creator.upgradeSo.bishopLosSprite;
                        break;
                    case Piece.King:
                        buttonImage.sprite = Creator.upgradeSo.kingLosSprite;
                        break;
                    case Piece.Knight:
                        buttonImage.sprite = Creator.upgradeSo.knightLosSprite;
                        break;
                    case Piece.Pawn:
                        buttonImage.sprite = Creator.upgradeSo.pawnLosSprite;
                        break;
                    case Piece.Queen:
                        buttonImage.sprite = Creator.upgradeSo.queenLosSprite;
                        break;
                    case Piece.Rook:
                        buttonImage.sprite = Creator.upgradeSo.rookLosSprite;
                        break;
                }
                break;
            case ArtefactTypes.UseCapturedPieceStraightAway:
                buttonImage.sprite = Creator.upgradeSo.useCapturedPieceStraightAwaySprite;
                break;
        }
        buttonExplanationText.text = _artefactTypeExplanationText[artefactType];
    }

    private void ApplyUpgrade(UpgradeTypes upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeTypes.IncreaseBaseMultiplierAmount:
                Creator.upgradeSo.baseMultiplier += 1;
                _pauseUISystem.UpdateBaseMultiplierText();
                _pauseUISystem.ShowUpgradeNotificationImage();
                break;
            case UpgradeTypes.IncreasePromotionXPGain:
                List<Piece> promoPieces = Creator.upgradeSo.promotionXPGain.Keys.ToList();
                foreach (Piece promoPiece in promoPieces)
                {
                    Creator.upgradeSo.promotionXPGain[promoPiece] += 1;
                }
                _playerSystem.SetPromoXpGainText(); //Just in case we capture a pawn and can then upgrade
                break;
            case UpgradeTypes.IncreaseBaseAmountGained:
                List<Piece> pieces = Creator.upgradeSo.capturePieceXPGain.Keys.ToList();
                foreach (Piece piece in pieces)
                {
                    Creator.upgradeSo.capturePieceXPGain[piece] += 1;
                }
                _pauseUISystem.UpdateCaptureBonusText();
                _pauseUISystem.ShowUpgradeNotificationImage();
                break;
        }
        _upgradeSelectedPlayer.PlayFeedbacks();
    }

    public void Show()
    {
        _choosingUpgrade = true;

        if (Creator.upgradeSo.guaranteeArtefactInUpgrade)
        {
            _isArtefact = true;
        }
        else
        {
            List<bool> isArtefactChance = new()
            {
                true,
                false,
                false,
                false
            };

            int artefactChanceIndex = Random.Range(0, isArtefactChance.Count);
            _isArtefact = isArtefactChance[artefactChanceIndex];
        }

        if (_isArtefact)
        {
            List<bool> isArtefactRightChance = new()
            {
                true,
                false
            };
            
            int artefactRightChanceIndex = Random.Range(0, isArtefactRightChance.Count);
            _isArtefactUpgradeTwo = isArtefactRightChance[artefactRightChanceIndex];

            List<ArtefactTypes> artefactTypesList = _artefactTypes.ToList();

            foreach (ArtefactTypes artefactTypes in Creator.upgradeSo.artefactsChosen)
            {
                artefactTypesList.Remove(artefactTypes);
            }

            int artefactChosenIndex = Random.Range(0, artefactTypesList.Count);
            _artefactChosen = artefactTypesList[artefactChosenIndex];

            if (_artefactChosen == ArtefactTypes.EnemyLineOfSight)
            {
                List<Piece> pieces = new()
                {
                    Piece.Bishop,
                    Piece.King,
                    Piece.Knight,
                    Piece.Pawn,
                    Piece.Queen,
                    Piece.Rook
                };
                
                int losPieceChosenIndex = Random.Range(0, pieces.Count);
                _losPieceChosen = pieces[losPieceChosenIndex];
            }

            if (_isArtefactUpgradeTwo)
            {
                SetArtefactButtonData(_artefactChosen, _losPieceChosen, _upgradeTwoImage, _upgradeTwoExplanationText);
            }
            else
            {
                SetArtefactButtonData(_artefactChosen, _losPieceChosen, _upgradeOneImage, _upgradeOneExplanationText);
            }
        }

        List<UpgradeTypes> upgradesTypes = _upgradeTypes.ToList();
        
        if (_isArtefact && !_isArtefactUpgradeTwo)
        {
            //We have an artefact and it overrides upgrade one
            //Therefore in this block of code we create an upgrade for upgrade two
            int upgradeTwoIndex = Random.Range(0, upgradesTypes.Count);
        
            _upgradeTwoType = upgradesTypes[upgradeTwoIndex];
        
            SetUpgradeButtonData(_upgradeTwoType, _upgradeTwoImage, _upgradeTwoExplanationText);
        }
        else if(_isArtefact && _isArtefactUpgradeTwo)
        {
            //We have an artefact and it overrides upgrade two
            //Therefore in this block of code we create an upgrade for upgrade one
            int upgradeOneIndex = Random.Range(0, upgradesTypes.Count);
            _upgradeOneType = upgradesTypes[upgradeOneIndex];
        
            SetUpgradeButtonData(_upgradeOneType, _upgradeOneImage, _upgradeOneExplanationText);
        }
        else
        {
            //There is NO artefact. Therefore we create an upgrade for button one AND two
            
            int upgradeOneIndex = Random.Range(0, upgradesTypes.Count);
            _upgradeOneType = upgradesTypes[upgradeOneIndex];
        
            SetUpgradeButtonData(_upgradeOneType, _upgradeOneImage, _upgradeOneExplanationText);
            
            upgradesTypes.Remove(upgradesTypes[upgradeOneIndex]);
            
            int upgradeTwoIndex = Random.Range(0, upgradesTypes.Count);
        
            _upgradeTwoType = upgradesTypes[upgradeTwoIndex];
        
            SetUpgradeButtonData(_upgradeTwoType, _upgradeTwoImage, _upgradeTwoExplanationText);
        }
        
        _upgradeUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _upgradeUI.gameObject.SetActive(false);

        _choosingUpgrade = false;
    }

    public bool ChoosingUpgrade()
    {
        return _choosingUpgrade;
    }
}
