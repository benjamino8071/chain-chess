using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElXPBarUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElUpgradeUISystem _upgradeUISystem;
    private ElAudioSystem _audioSystem;
    
    private MMProgressBar _progressBar;

    private MMF_Player _upgradeButtonPressedPlayer;

    private TextMeshProUGUI _multiplierAmountText;
    private TextMeshProUGUI _levelNumberText;
    private TextMeshProUGUI _xpEarntInLevelText;

    private Button _upgradeButton;
    
    private float _amount;
    private float _amountRequiredToUpgrade = 10;

    private float _multiplier;

    private float _consecCaptureAmount;
    
    public override void GameStart(ElCreator creator)
    {
        base.GameStart(creator);

        _upgradeUISystem = creator.GetDependency<ElUpgradeUISystem>();
        _audioSystem = creator.GetDependency<ElAudioSystem>();
        _playerSystem = creator.GetDependency<ElPlayerSystem>();
        
        Transform guiRight = creator.GetFirstObjectWithName(AllTagNames.GUIRight);
        _progressBar = guiRight.GetComponentInChildren<MMProgressBar>();

        Transform upgradeButton = creator.GetChildObjectByName(guiRight.gameObject, AllTagNames.UpgradeButton);
        _upgradeButton = upgradeButton.GetComponent<Button>();
        _upgradeButton.onClick.AddListener(() =>
        {
            _upgradeButtonPressedPlayer.PlayFeedbacks();
            HideUpgradeButton();
            ShowLevelUpgrade();
        });
        HideUpgradeButton();

        Transform upgradeButtonPressedPlayer =
            creator.GetChildObjectByName(guiRight.gameObject, AllTagNames.MmfUpgradeButtonPlayer);
        _upgradeButtonPressedPlayer = upgradeButtonPressedPlayer.GetComponent<MMF_Player>();
        
        Transform multiplierAmountText =
            creator.GetChildObjectByName(guiRight.gameObject, AllTagNames.MultiplierAmount);
        _multiplierAmountText = multiplierAmountText.GetComponent<TextMeshProUGUI>();

        Transform levelNumberText = creator.GetChildObjectByName(guiRight.gameObject, AllTagNames.LevelNumber);
        _levelNumberText = levelNumberText.GetComponent<TextMeshProUGUI>();
        
        Transform xpEarntInLevelText = creator.GetChildObjectByName(guiRight.gameObject, AllTagNames.XpPoints);
        _xpEarntInLevelText = xpEarntInLevelText.GetComponent<TextMeshProUGUI>();

        Creator.upgradeSo.levelNumber = Creator.upgradeSo.levelNumberOnRoomEnter;

        for (int i = 1; i < Creator.upgradeSo.levelNumber; i++)
        {
            _amountRequiredToUpgrade *= 2;
        }

        if (Creator.upgradeSo.xpAmountOnNextLevelEnter > 0)
        {
            SetBar(Creator.upgradeSo.xpAmountOnNextLevelEnter);
            Creator.upgradeSo.xpAmountOnNextLevelEnter = 0;
        }
        
        _levelNumberText.text = $"Level {creator.upgradeSo.levelNumber}";
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        
        ResetMultiplier();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _upgradeButton.gameObject.SetActive(_playerSystem.GetState() == ElPlayerSystem.States.Idle && _amount >= _amountRequiredToUpgrade);
    }

    public void IncreaseProgressBar(float amount, bool playSfx)
    {
        amount *= _multiplier;

        _playerSystem.PlayFloatingTextPlayer(amount);
        
        _amount += amount;
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
        
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        
        if (_amount >= _amountRequiredToUpgrade)
        {
            ShowUpgradeButton();
        }

        if (playSfx)
        {
            //float numOfCaps = Mathf.Log(_multiplier, 2);
            
            float pitch = Mathf.Clamp(0.9f + _consecCaptureAmount / 50, 0.9f, 2f);
            _audioSystem.PlayTimeAddedSfx(pitch);
        }
        _consecCaptureAmount++;
        
        _multiplier *= 2;
        float waveAmp = Mathf.Clamp(0.01f * _multiplier, 0.01f, 0.1f);
        _multiplierAmountText.text = $"<wave a={waveAmp}>{_multiplier:0.##}\u00d7</wave>";
    }

    public void IncreaseProgressBarNoMultiplier(float amount)
    {
        _amount += amount;
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
        
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        
        if (_amount >= _amountRequiredToUpgrade)
        {
            ShowUpgradeButton();
        }
        
        _audioSystem.PlayTimeAddedSfx(0.9f);
    }
    
    public void SetBar(float amount)
    {
        _amount = amount;
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
        
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";

        if (_amount >= _amountRequiredToUpgrade)
        {
            ShowUpgradeButton();
        }
    }
    
    public void ResetMultiplier()
    {
        _consecCaptureAmount = 0;
        _multiplier = Creator.upgradeSo.baseMultiplier;
        _multiplierAmountText.text = $"<wave a={0.01f}>{_multiplier:0.##}\u00d7</wave>";
    }

    public void ShowUpgradeButton()
    {
        _upgradeButton.gameObject.SetActive(true);
    }

    public void HideUpgradeButton()
    {
        _upgradeButton.gameObject.SetActive(false);
    }

    public void ShowLevelUpgrade()
    {
        _upgradeUISystem.Show();
        float amountLeftOver = _amount - _amountRequiredToUpgrade;
        
        _amountRequiredToUpgrade *= 2;
        
        Creator.upgradeSo.levelNumber++;
        _levelNumberText.text = $"Level {Creator.upgradeSo.levelNumber}";
        
        SetBar(amountLeftOver);
    }

    public void UpgradeByFullLevel()
    {
        //We get the amount needed to upgrade.
        //Then we give the player ALL of that to their amount
        SetBar(_amount + _amountRequiredToUpgrade);
        
        _audioSystem.PlayTimeAddedSfx(0.9f);
    }

    public void SaveXpAmount()
    {
        Creator.upgradeSo.xpAmountOnNextLevelEnter = _amount;
    }

    public void SaveLevelNumber()
    {
        Creator.upgradeSo.levelNumberOnRoomEnter = Creator.upgradeSo.levelNumber;
    }
}
