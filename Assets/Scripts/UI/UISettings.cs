using System.Collections.Generic;
using Michsky.MUIP;
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

    private SpriteRenderer _tiles;
    private SpriteRenderer _edge;
    
    private bool _canShow;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        Transform tiles = Creator.GetFirstObjectWithName(AllTagNames.Tiles);
        _tiles = tiles.GetComponent<SpriteRenderer>();

        Transform edge = Creator.GetFirstObjectWithName(AllTagNames.Edge);
        _edge = edge.GetComponent<SpriteRenderer>();

        ButtonManager audioButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonAudio);
        audioButton.onClick.AddListener(() =>
        {
            Creator.settingsSo.sound = !Creator.settingsSo.sound;
            
            UpdateSoundSetting();
            
            audioButton.SetIcon(Creator.settingsSo.sound ? Creator.miscUiSo.audioOnSprite : Creator.miscUiSo.audioOffSprite);
        });

        Transform buttonsParent = Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.ColourVariantsParent);
        foreach (BoardVariant boardVariant in Creator.boardSo.boardVariants)
        {
            GameObject buttonGo = Creator.InstantiateGameObjectWithParent(Creator.colourVariantButtonPrefab, buttonsParent);
            
            ButtonManager buttonManager = buttonGo.GetComponent<ButtonManager>();
            UIGradient[] uiGradients = buttonGo.GetComponentsInChildren<UIGradient>();
            foreach (UIGradient uiGradient in uiGradients)
            {
                uiGradient.EffectGradient = boardVariant.swappedColourGradient;
                uiGradient.GradientType = UIGradient.Type.Horizontal;
            }
            buttonManager.onClick.AddListener(() =>
            {
                _tiles.sprite = boardVariant.board;
                _edge.color = boardVariant.edgeColur;
            });
        }
        
        UpdateSoundSetting();
        
        Hide();
    }
    
    private void UpdateSoundSetting()
    {
        MMSoundManager.Instance.SetVolumeMaster(Creator.settingsSo.sound ? 1 : 0);
    }
}
