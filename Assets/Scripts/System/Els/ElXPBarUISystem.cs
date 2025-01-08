using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class ElXPBarUISystem : ElDependency
{
    //private UpgradeGUISystem _upgradeGUISystem;
    private ElAudioSystem _audioSystem;
    
    private MMProgressBar _progressBar;

    private TextMeshProUGUI _multiplierAmountText;
    private TextMeshProUGUI _levelNumberText;
    private TextMeshProUGUI _xpEarntInLevelText;
    
    private float _amount;
    private float _amountRequiredToUpgrade = 10;

    private float _multiplier;

    private int _levelNumber = 1;
    
    public override void GameStart(ElCreator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<ElAudioSystem>();

        //_upgradeGUISystem = creator.GetDependency<UpgradeGUISystem>();

        Transform guiBottom = creator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        _progressBar = guiBottom.GetComponentInChildren<MMProgressBar>();

        Transform multiplierAmountText =
            creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.MultiplierAmount);
        _multiplierAmountText = multiplierAmountText.GetComponent<TextMeshProUGUI>();

        Transform levelNumberText = creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.LevelNumber);
        _levelNumberText = levelNumberText.GetComponent<TextMeshProUGUI>();
        
        Transform xpEarntInLevelText = creator.GetChildObjectByName(guiBottom.gameObject, AllTagNames.XpPoints);
        _xpEarntInLevelText = xpEarntInLevelText.GetComponent<TextMeshProUGUI>();

        _levelNumberText.text = $"Level {_levelNumber}";
        _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
        
        ResetMultiplier();
    }
    
    public void IncreaseProgressBar(float amount, bool playSfx, Vector3 playerPos, MMF_Player floatingTextPlayer)
    {
        amount *= _multiplier;

        floatingTextPlayer.PlayFeedbacks(playerPos, amount);
        
        _amount += amount;
        
        _progressBar.UpdateBar(_amount, 0, _amountRequiredToUpgrade);
        
        if (_amount >= _amountRequiredToUpgrade)
        {
            _amount = _amountRequiredToUpgrade;
            _xpEarntInLevelText.text = $"XP: {_amount:0}/{_amountRequiredToUpgrade:0}";
            
            LevelUpgrade();
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

    public void ResetBar()
    {
        _amount = 0;
        _xpEarntInLevelText.text = $"XP: 0/{_amountRequiredToUpgrade:0}";
        
        _progressBar.SetBar(0, 0, _amountRequiredToUpgrade);
    }
    
    public void ResetMultiplier()
    {
        _multiplier = 1;
        _multiplierAmountText.text = $"<wave a={0.01f}>Multiplier: 1x</wave>";
    }

    public void LevelUpgrade()
    {
        _levelNumber++;
        _amountRequiredToUpgrade *= 2;

        _levelNumberText.text = $"Level {_levelNumber}";
        
        ResetBar();
    }
}
