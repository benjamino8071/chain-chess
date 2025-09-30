using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
            Creator.saveDataSo.audio = !Creator.saveDataSo.audio;
            
            UpdateSoundSetting(Creator.saveDataSo.audio);
            
            audioButton.SetIcon(Creator.saveDataSo.audio ? Creator.miscUiSo.audioOnSprite : Creator.miscUiSo.audioOffSprite);
        });

        ButtonManager fullscreenButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonFullscreen);
        fullscreenButton.onClick.AddListener(() =>
        {
            Creator.saveDataSo.isFullscreen = !Creator.saveDataSo.isFullscreen;
            
            UpdateFullscreen(Creator.saveDataSo.isFullscreen);
        });
        
        ButtonManager deleteButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonDelete);
        deleteButton.onClick.AddListener(() =>
        {
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
                _tiles.sprite = boardVariant.board;
                _edge.color = boardVariant.edgeColur;
            });
        }
        
        UpdateFullscreen(Creator.saveDataSo.isFullscreen);
        
        UpdateSoundSetting(Creator.saveDataSo.audio);
        
        Creator.inputSo.exitFullscreen.action.performed += ExitFullscreen_Performed;
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

    private int _width = 640;
    private int _height = 480;
    
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
