using System.Collections;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class ElXPBarUISystem : ElDependency
{
    //private UpgradeGUISystem _upgradeGUISystem;
    
    private MMProgressBar _progressBar;

    private float _amount;
    
    public override void GameStart(ElCreator creator)
    {
        base.GameStart(creator);
        

        //_upgradeGUISystem = creator.GetDependency<UpgradeGUISystem>();

        GameObject guiButton = GameObject.FindWithTag("GUIBottom");
        _progressBar = guiButton.GetComponentInChildren<MMProgressBar>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">MUST BE BETWEEN 0 AND 1</param>
    public void IncreaseProgressBar(float amount)
    {
        amount *= Creator.timerSo.timerMultiplier;
        
        _amount = Mathf.Clamp01(_amount + amount);
        
        Debug.Log("AMOUNT INCREASE: "+amount);
        
        _progressBar.UpdateBar01(_amount);

        if (_amount >= 1)
        {
            //_upgradeGUISystem.Show();
            ResetBar();
        }
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
}
