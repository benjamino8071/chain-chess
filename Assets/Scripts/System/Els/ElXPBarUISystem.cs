using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class ElXPBarUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElUpgradeUISystem _upgradeUISystem;
    private ElAudioSystem _audioSystem;
    
    private MMProgressBar _progressBar;

    private TextMeshProUGUI _multiplierAmountText;
    private TextMeshProUGUI _levelNumberText;
    private TextMeshProUGUI _xpEarntInLevelText;
    
    private float _amount;
    private float _amountRequiredToUpgrade = 10;

    private float _multiplier;
    
    public override void GameStart(ElCreator creator)
    {
        base.GameStart(creator);

        _upgradeUISystem = creator.GetDependency<ElUpgradeUISystem>();
        _audioSystem = creator.GetDependency<ElAudioSystem>();
        _playerSystem = creator.GetDependency<ElPlayerSystem>();
        
        Transform guiBottom = creator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        _progressBar = guiBottom.GetComponentInChildren<MMProgressBar>();

        Transform multiplierAmountText =
            creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.MultiplierAmount);
        _multiplierAmountText = multiplierAmountText.GetComponent<TextMeshProUGUI>();

        Transform levelNumberText = creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.LevelNumber);
        _levelNumberText = levelNumberText.GetComponent<TextMeshProUGUI>();
        
        Transform xpEarntInLevelText = creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.XpPoints);
        _xpEarntInLevelText = xpEarntInLevelText.GetComponent<TextMeshProUGUI>();

        Creator.xpBarSo.levelNumber = Creator.xpBarSo.levelNumberOnRoomEnter;
        
        _amountRequiredToUpgrade *= Creator.xpBarSo.levelNumber;
        
        _levelNumberText.text = $"Level {creator.xpBarSo.levelNumber}";
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        
        ResetMultiplier();
    }
    
    public void IncreaseProgressBar(float amount, bool playSfx)
    {
        amount *= _multiplier;

        _playerSystem.PlayFloatingTextPlayer(amount);
        
        _amount += amount;
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
        
        if (_amount >= _amountRequiredToUpgrade)
        {
            float amountLeftOver = _amount - _amountRequiredToUpgrade;
            
            _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
            
            LevelUpgrade();
            ResetBar(amountLeftOver);
        }
        else
        {
            _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        }

        if (playSfx)
        {
            float numOfCaps = Mathf.Log(_multiplier, 2);
            
            float pitch = Mathf.Clamp(0.9f + numOfCaps / 50, 0.9f, 2f);
            _audioSystem.PlayTimeAddedSfx(pitch);
        }
        
        _multiplier *= 2;
        float waveAmp = Mathf.Clamp(0.01f * _multiplier, 0.01f, 0.1f);
        _multiplierAmountText.text = $"<wave a={waveAmp}>Multiplier: {_multiplier:0.##}x</wave>";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">MUST BE BETWEEN 0 AND 1</param>
    public void DecreaseProgressBar(float amount)
    {
        _amount = Mathf.Clamp01(_amount - amount);
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
    }

    public void ResetBar(float amountToAddOnReset)
    {
        _amount = amountToAddOnReset;
        _xpEarntInLevelText.text = $"XP: {amountToAddOnReset:0}/{_amountRequiredToUpgrade:0}";
        
        _progressBar.SetBar(amountToAddOnReset, 0, _amountRequiredToUpgrade);
    }
    
    public void ResetMultiplier()
    {
        _multiplier = Creator.xpBarSo.baseMultiplier;
        _multiplierAmountText.text = $"<wave a={0.01f}>Multiplier: {_multiplier:0.##}\u00d7</wave>";
    }

    public void LevelUpgrade()
    {
        Creator.xpBarSo.levelNumber++;
        _amountRequiredToUpgrade *= 2;

        _levelNumberText.text = $"Level {Creator.xpBarSo.levelNumber}";
        
        _upgradeUISystem.Show();
    }
}
