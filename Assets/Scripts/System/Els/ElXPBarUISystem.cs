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

    private float _amount;

    private float _multiplier;
    
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
        
        ResetMultiplier();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">MUST BE BETWEEN 0 AND 1</param>
    /// <param name="playSfx"></param>
    public void IncreaseProgressBar(float amount, bool playSfx)
    {
        amount *= _multiplier;

        _amount = Mathf.Clamp01(_amount + amount);

        _progressBar.UpdateBar01(_amount);

        if (_amount >= 1)
        {
            //_upgradeGUISystem.Show();
            ResetBar();
        }

        if (playSfx)
        {
            float numOfCaps = Mathf.Log(_multiplier, 2);
            Debug.Log("Num of caps: "+numOfCaps);
            
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
        
        _progressBar.UpdateBar01(_amount);
    }

    public void ResetBar()
    {
        _amount = 0;
        _progressBar.SetBar01(0);
    }
    
    public void ResetMultiplier()
    {
        _multiplier = 1;
        _multiplierAmountText.text = $"<wave a={0.01f}>Multiplier: 1x</wave>";
    }
}
