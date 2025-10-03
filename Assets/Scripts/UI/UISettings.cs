using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettings : UIPanel
{
    private TurnSystem _turnSystem;
    private BoardSystem _boardSystem;
    
    private Transform _settingsGui;

    private Button _uiBottomResetButton;
    private Button _settingsButton;

    private SpriteRenderer _tiles;
    private SpriteRenderer _edge;
    private SpriteRenderer _background;
    
    private bool _canShow;
    
    private int _width;
    private int _height;

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
            
            Creator.SaveToDisk();
        });

        ButtonManager fullscreenButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonFullscreen);
        fullscreenButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Creator.saveDataSo.isFullscreen = !Creator.saveDataSo.isFullscreen;
            
            UpdateFullscreen(Creator.saveDataSo.isFullscreen);
            
            Creator.SaveToDisk();
        });
        
        ButtonManager deleteButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonDelete);
        deleteButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Creator.DeleteOnDisk();
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
                
                Creator.SaveToDisk();
            });
        }

        _width = Creator.saveDataSo.windowWidth;
        _height = Creator.saveDataSo.windowHeight;
        
        UpdateFullscreen(Creator.saveDataSo.isFullscreen);
        
        UpdateSoundSetting(Creator.saveDataSo.audio);
        
        UpdateBoardVariant(Creator.saveDataSo.boardVariant);
        
        Creator.inputSo.exitFullscreen.action.performed += ExitFullscreen_Performed;
    }

    private void UpdateBoardVariant(BoardVariant boardVariant)
    {
        _tiles.sprite = boardVariant.board;
        _edge.color = boardVariant.edgeColur;
        _background.color = boardVariant.backgroundColour;
        _uiSystem.SetBadAspectRatioColour(boardVariant.backgroundColour);
    }

    private void ExitFullscreen_Performed(InputAction.CallbackContext obj)
    {
        if (!Creator.saveDataSo.isFullscreen)
        {
            return;
        }

        Creator.saveDataSo.isFullscreen = false;
        UpdateFullscreen(false);
    }
    
    private void UpdateFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            _width = Screen.width;
            _height = Screen.height;
            
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(_width, _height, FullScreenMode.Windowed);
        }
        Screen.fullScreen = isFullscreen;
    }
    
    private void UpdateSoundSetting(bool audioOn)
    {
        MMSoundManager.Instance.SetVolumeMaster(audioOn ? 1 : 0);
    }
}
