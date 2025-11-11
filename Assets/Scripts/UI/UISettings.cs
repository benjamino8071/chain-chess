using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : UIPanel
{
    private BlackSystem _blackSystem;
    
    private Transform _settingsGui;

    private Button _uiBottomResetButton;
    private Button _settingsButton;

    private SpriteRenderer _tiles;
    private SpriteRenderer _edge;
    private SpriteRenderer _background;
    
    private bool _canShow;
    
    private int _width;
    private int _height;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _blackSystem = creator.GetDependency<BlackSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        Transform tiles = Creator.GetFirstObjectWithName(AllTagNames.Tiles);
        _tiles = tiles.GetComponent<SpriteRenderer>();

        Transform edge = Creator.GetFirstObjectWithName(AllTagNames.Edge);
        _edge = edge.GetComponent<SpriteRenderer>();
        
        _background = Creator.GetChildComponentByName<SpriteRenderer>(Creator.mainCam.gameObject, AllTagNames.BackgroundImage);

        ButtonManager audioButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonAudio);
        audioButton.onClick.AddListener(() =>
        {
            Creator.saveDataSo.audio = !Creator.saveDataSo.audio;
            
            UpdateSoundSetting(Creator.saveDataSo.audio);
            
            audioButton.SetIcon(Creator.saveDataSo.audio ? Creator.miscUiSo.audioOnSprite : Creator.miscUiSo.audioOffSprite);
            
            _audioSystem.PlayUIClickSfx();
        });

        ButtonManager abilityTextButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonAbilityText);
        abilityTextButton.onClick.AddListener(() =>
        {
            Creator.saveDataSo.showAbilityText = !Creator.saveDataSo.showAbilityText;

            _blackSystem.SetAbilityTexts(Creator.saveDataSo.showAbilityText);

            _audioSystem.PlayUIClickSfx();
        });

        List<ButtonManager> colourVariants =
            Creator.GetChildComponentsByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonColourVariants);
        if (colourVariants.Count != Creator.boardSo.boardVariants.Count)
        {
            Debug.LogError("Colour Variants button count must equal board variant count.");
            return;
        }

        for (int i = 0; i < colourVariants.Count; i++)
        {
            ButtonManager button = colourVariants[i];
            BoardVariant boardVariant = Creator.boardSo.boardVariants[i];
            
            UIGradient[] uiGradients = button.GetComponentsInChildren<UIGradient>();
            foreach (UIGradient uiGradient in uiGradients)
            {
                uiGradient.EffectGradient = boardVariant.swappedColourGradient;
                uiGradient.GradientType = UIGradient.Type.Horizontal;
            }
            button.onClick.AddListener(() =>
            {
                _audioSystem.PlayUIClickSfx();
                
                Creator.saveDataSo.boardVariant = boardVariant;
                
                UpdateBoardVariant(boardVariant);
            });
        }

        _width = Creator.saveDataSo.windowWidth;
        _height = Creator.saveDataSo.windowHeight;
        
        UpdateSoundSetting(Creator.saveDataSo.audio);
        
        UpdateBoardVariant(Creator.saveDataSo.boardVariant);
    }

    private void UpdateBoardVariant(BoardVariant boardVariant)
    {
        _tiles.sprite = boardVariant.board;
        _edge.color = boardVariant.edgeColur;
        _background.color = boardVariant.backgroundColour;
        _uiSystem.SetBadAspectRatioColour(boardVariant.backgroundColour);
    }
    
    private void UpdateSoundSetting(bool audioOn)
    {
        MMSoundManager.Instance.SetVolumeMaster(audioOn ? 1 : 0);
    }
}
