using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;
    private BoardSystem _boardSystem;
    
    private Transform _settingsGui;

    private Button _uiBottomResetButton;
    private Button _settingsButton;
    
    private bool _canShow;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        UpdateSoundSetting();
        
        Hide();
    }
    
    private void UpdateSoundSetting()
    {
        MMSoundManager.Instance.SetVolumeMaster(Creator.settingsSo.sound ? 1 : 0);
    }
}
