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

    private bool _choosingUpgrade;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        _pauseUISystem = elCreator.GetDependency<ElPauseUISystem>();
        
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
        ApplyUpgrade(_upgradeOneType);
        Hide();
    }

    private void UpgradeTwoButtonPressed()
    {
        ApplyUpgrade(_upgradeTwoType);
        Hide();
    }
    
    private void SetUpgradeButtonData(UpgradeTypes upgradeType, Image buttonImage, TextMeshProUGUI buttonExplanationText)
    {
        switch (upgradeType)
        {
            case UpgradeTypes.IncreaseBaseMultiplierAmount:
                buttonImage.sprite = Creator.xpBarSo.increaseMultiplierAmountSprite;
                break;
            case UpgradeTypes.IncreasePromotionXPGain:
                buttonImage.sprite = Creator.xpBarSo.increasePromoXpGainSprite;
                break;
            case UpgradeTypes.IncreaseBaseAmountGained:
                buttonImage.sprite = Creator.xpBarSo.increaseBaseAmountGainedSprite;
                break;
        }
        buttonExplanationText.text = _upgradeTypeExplanationText[upgradeType];
    }

    private void ApplyUpgrade(UpgradeTypes upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeTypes.IncreaseBaseMultiplierAmount:
                Creator.xpBarSo.baseMultiplier += 1;
                _pauseUISystem.UpdateBaseMultiplierText();
                _pauseUISystem.ShowUpgradeNotificationImage();
                break;
            case UpgradeTypes.IncreasePromotionXPGain:
                List<Piece> promoPieces = Creator.xpBarSo.promotionXPGain.Keys.ToList();
                foreach (Piece promoPiece in promoPieces)
                {
                    Creator.xpBarSo.promotionXPGain[promoPiece] += 1;
                }
                _playerSystem.SetPromoXpGainText(); //Just in case we capture a pawn and can then upgrade
                break;
            case UpgradeTypes.IncreaseBaseAmountGained:
                List<Piece> pieces = Creator.xpBarSo.capturePieceXPGain.Keys.ToList();
                foreach (Piece piece in pieces)
                {
                    Creator.xpBarSo.capturePieceXPGain[piece] += 1;
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

        List<UpgradeTypes> upgradesTypes = _upgradeTypes.ToList();
        
        int upgradeOneIndex = Random.Range(0, upgradesTypes.Count);
        _upgradeOneType = upgradesTypes[upgradeOneIndex];
        
        SetUpgradeButtonData(_upgradeOneType, _upgradeOneImage, _upgradeOneExplanationText);

        upgradesTypes.Remove(upgradesTypes[upgradeOneIndex]);
        int upgradeTwoIndex = Random.Range(0, upgradesTypes.Count);
        
        _upgradeTwoType = upgradesTypes[upgradeTwoIndex];
        
        SetUpgradeButtonData(_upgradeTwoType, _upgradeTwoImage, _upgradeTwoExplanationText);
        
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
