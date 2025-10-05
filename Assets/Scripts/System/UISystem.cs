using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.FeedbacksForThirdParty;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISystem : Dependency
{
    public List<RaycastResult> objectsUnderMouse => _currentCanvas.objectsUnderMouse;

    public AllTagNames leftBotSidePanelOpen => _currentCanvas.leftBotSidePanelOpen;
    public AllTagNames rightTopSidePanelOpen => _currentCanvas.rightTopSidePanelOpen;
    
    public Image leftBotBackground => _currentCanvas.leftBotBackground;
    public Image rightTopBackground => _currentCanvas.rightTopBackground;
    
    public AllTagNames canvasType => _canvasType;

    private AudioSystem _audioSystem;
    
    private UICanvas _landscapeCanvas;
    private UICanvas _portraitCanvas;

    private UICanvas _currentCanvas;

    private Transform _badAspectRatioCanvas;
    private Image _badAspectRatioBackground;
    
    private Transform _thankYouForPlayingCanvas;
    private Image _thankYouForPlayingBackground;
    private ScaleTween _thankYouForPlayingTween;
    
    private AllTagNames _canvasType;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        
        _badAspectRatioCanvas = creator.GetFirstObjectWithName(AllTagNames.BadAspectRatio);
        _badAspectRatioBackground =
            creator.GetChildComponentByName<Image>(_badAspectRatioCanvas.gameObject, AllTagNames.Image);
        
        _thankYouForPlayingCanvas = creator.GetFirstObjectWithName(AllTagNames.ThankYouForPlaying);
        _thankYouForPlayingBackground = creator.GetChildComponentByName<Image>(_thankYouForPlayingCanvas.gameObject, AllTagNames.Image);
        _thankYouForPlayingTween = _thankYouForPlayingCanvas.GetComponentInChildren<ScaleTween>();
        
        ButtonManager okButton =
            creator.GetChildComponentByName<ButtonManager>(_thankYouForPlayingCanvas.gameObject,
                AllTagNames.ButtonAccept);
        okButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();

            _thankYouForPlayingCanvas.gameObject.SetActive(false);
            _canvasType = AllTagNames.None;
        });
        
        _landscapeCanvas = new();
        _landscapeCanvas.AssignCanvas(AllTagNames.LandscapeMode);
        _landscapeCanvas.GameStart(creator);
        _landscapeCanvas.SetVisibility(false);
        
        _portraitCanvas = new();
        _portraitCanvas.AssignCanvas(AllTagNames.PortraitMode);
        _portraitCanvas.GameStart(creator);
        _portraitCanvas.SetVisibility(false);
        
        UpdateCurrentCanvas();
        
        _badAspectRatioCanvas.gameObject.SetActive(false);
        _thankYouForPlayingCanvas.gameObject.SetActive(false);
        
        //Turn system handles showing chain
        SetHomescreen(true);
    }
    
    public override void GameUpdate(float dt)
    {
        UpdateCurrentCanvas();
        
        _landscapeCanvas.GameUpdate(dt);
        _portraitCanvas.GameUpdate(dt);
    }

    private void UpdateCurrentCanvas()
    {
        float width = Screen.width;
        float height = Screen.height;
        
        float aspect = width / height;

        if (_canvasType is AllTagNames.ThankYouForPlaying)
        {
            
        }
        else if (aspect < Creator.settingsSo.absoluteMinimumAspectRatio && _canvasType != AllTagNames.BadAspectRatio)
        {
            _badAspectRatioCanvas.gameObject.SetActive(true);
            
            _landscapeCanvas.SetVisibility(false);
            _portraitCanvas.SetVisibility(false);

            _canvasType = AllTagNames.BadAspectRatio;
        }
        else if (aspect < Creator.settingsSo.aspectRatioToChangeValue && _canvasType != AllTagNames.PortraitMode)
        {
            _badAspectRatioCanvas.gameObject.SetActive(false);
            
            _landscapeCanvas.SetVisibility(false);
            _portraitCanvas.SetVisibility(true);
            
            _currentCanvas = _portraitCanvas;
            Creator.mainCam.orthographicSize = Creator.settingsSo.portraitModeSettings.cameraSize;
            
            _canvasType = AllTagNames.PortraitMode;
        }
        else if (aspect >= Creator.settingsSo.aspectRatioToChangeValue && _canvasType != AllTagNames.LandscapeMode)
        {
            _badAspectRatioCanvas.gameObject.SetActive(false);
            
            _portraitCanvas.SetVisibility(false);
            _landscapeCanvas.SetVisibility(true);
            
            _currentCanvas = _landscapeCanvas;
            Creator.mainCam.orthographicSize = Creator.settingsSo.landscapeModeSettings.cameraSize;
            
            _canvasType = AllTagNames.LandscapeMode;
        }
    }

    public void ShowLeftBotSideUI(AllTagNames uiTag)
    {
        _landscapeCanvas.ShowLeftBotSideUI(uiTag);
        _portraitCanvas.ShowLeftBotSideUI(uiTag);
    }
    
    private void HideLeftBotSideUINoTween()
    {
        _landscapeCanvas.HideLeftBotSideUINoTween();
        _portraitCanvas.HideLeftBotSideUINoTween();
    }
    
    public void HideLeftBotSideUI()
    {
        _landscapeCanvas.HideLeftBotSideUI();
        _portraitCanvas.HideLeftBotSideUI();
    }

    public void SetHomescreen(bool show)
    {
        List<UICurrentLevelMenu> currentLevelMenus = GetUI<UICurrentLevelMenu>();
        foreach (UICurrentLevelMenu currentLevelMenu in currentLevelMenus)
        {
            if (show)
            {
                currentLevelMenu.Hide();
            }
            else
            {
                currentLevelMenu.Show();
            }
        }
        List<UIBasicMenu> basicMenus = GetUI<UIBasicMenu>();
        foreach (UIBasicMenu basicMenu in basicMenus)
        {
            if (show)
            {
                basicMenu.Hide();
            }
            else
            {
                basicMenu.Show();
            }
        }

        if (show)
        {
            HideLeftBotSideUI();
            ShowRightTopSideUI(AllTagNames.UITitle);
        }
        else
        {
            if (Creator.saveDataSo.isFirstTime)
            {
                ShowLeftBotSideUI(AllTagNames.UIRulebook);
            }
            else
            {
                HideLeftBotSideUINoTween();
            }
            ShowRightTopSideUI(AllTagNames.UIChain);
        }
    }
    
    public void ShowRightTopSideUI(AllTagNames uiTag)
    {
        _landscapeCanvas.ShowRightTopSideUI(uiTag);
        _portraitCanvas.ShowRightTopSideUI(uiTag);
    }
    
    public void HideRightTopSideUI()
    {
        _landscapeCanvas.HideRightTopSideUI();
        _portraitCanvas.HideRightTopSideUI();
    }

    public List<T> GetUI<T>() where T : UIPanel
    {
        List<T> list = new(2)
        {
            _portraitCanvas.GetUI<T>(),
            _landscapeCanvas.GetUI<T>()
        };

        return list;
    }

    public void SetBadAspectRatioColour(Color color)
    {
        color.a = 0.2f;
        _badAspectRatioBackground.color = color;
        _thankYouForPlayingBackground.color = color;
    }

    public void ShowThankYouForPlayingUI()
    {
        _badAspectRatioCanvas.gameObject.SetActive(false);
            
        _landscapeCanvas.SetVisibility(false);
        _portraitCanvas.SetVisibility(false);
        
        _thankYouForPlayingCanvas.gameObject.SetActive(true);
        _thankYouForPlayingTween.PhaseIn();
        
        _canvasType = AllTagNames.ThankYouForPlaying;
    }
}
