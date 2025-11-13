using System;
using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : Dependency
{
    public AllTagNames leftBotSidePanelOpen => _currentCanvas.leftBotSidePanelOpen;
    public AllTagNames rightTopSidePanelOpen => _currentCanvas.rightTopSidePanelOpen;
    public AllTagNames canvasType => _canvasType;

    private AudioSystem _audioSystem;
    
    private UICanvas _landscapeCanvas;
    private UICanvas _portraitCanvas;

    private UICanvas _currentCanvas;

    private Transform _badAspectRatioCanvas;
    private Image _badAspectRatioBackground;
    
    private Transform _tyfpCanvas;
    private Image _tyfpBackground;
    private ScaleTween _tyfpTween;
    private TextMeshProUGUI _tyfpTotalTurnsText;
    private TextMeshProUGUI _tyfpTotalMovesText;
    private TextMeshProUGUI _tyfpTotalCapturesText;
    private TextMeshProUGUI _tyfpTotalTimeText;
    
    private AllTagNames _canvasType;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        
        _badAspectRatioCanvas = creator.GetFirstObjectWithName(AllTagNames.BadAspectRatio);
        _badAspectRatioBackground =
            creator.GetChildComponentByName<Image>(_badAspectRatioCanvas.gameObject, AllTagNames.Image);
        
        _tyfpCanvas = creator.GetFirstObjectWithName(AllTagNames.ThankYouForPlaying);
        _tyfpBackground = creator.GetChildComponentByName<Image>(_tyfpCanvas.gameObject, AllTagNames.Image);
        _tyfpTween = _tyfpCanvas.GetComponentInChildren<ScaleTween>();
        _tyfpTotalTurnsText =
            creator.GetChildComponentByName<TextMeshProUGUI>(_tyfpCanvas.gameObject, AllTagNames.TurnsText);
        _tyfpTotalMovesText =
            creator.GetChildComponentByName<TextMeshProUGUI>(_tyfpCanvas.gameObject, AllTagNames.MovesText);
        _tyfpTotalCapturesText =
            creator.GetChildComponentByName<TextMeshProUGUI>(_tyfpCanvas.gameObject, AllTagNames.CapturesText);
        _tyfpTotalTimeText =
            creator.GetChildComponentByName<TextMeshProUGUI>(_tyfpCanvas.gameObject, AllTagNames.TimeText);

        ButtonManager okButton =
            creator.GetChildComponentByName<ButtonManager>(_tyfpCanvas.gameObject,
                AllTagNames.ButtonAccept);
        okButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();

            _tyfpCanvas.gameObject.SetActive(false);
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
        _tyfpCanvas.gameObject.SetActive(false);
        
        //Turn system handles showing chain
        SetHomescreen(true);
    }
    
    public override void GameUpdate(float dt)
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _canvasType = _canvasType == AllTagNames.Hidden ? AllTagNames.None : AllTagNames.Hidden;
        }
        
        UpdateCurrentCanvas();
        
        _landscapeCanvas.GameUpdate(dt);
        _portraitCanvas.GameUpdate(dt);
    }

    private void UpdateCurrentCanvas()
    {
        float width = Screen.width;
        float height = Screen.height;
        
        float aspect = width / height;

        if (_canvasType == AllTagNames.Hidden)
        {
            _landscapeCanvas.SetVisibility(false);
            _portraitCanvas.SetVisibility(false);
        }
        else if (_canvasType == AllTagNames.ThankYouForPlaying)
        {
            //nothing here...
        }
        else if (aspect < Creator.settingsSo.absoluteMinimumAspectRatio)
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

    public void SetHomescreen(bool show)
    {
        if (show)
        {
            ShowRightTopSideUI(AllTagNames.UITitle);
            ShowLeftBotSideUI(AllTagNames.UICredits);
        }
        else
        {
            ShowLeftBotSideUI(Creator.saveDataSo.isFirstTime ? AllTagNames.UIRulebook : AllTagNames.UICurrentLevel);
            ShowRightTopSideUI(AllTagNames.UIChain);
        }
    }
    
    public void ShowRightTopSideUI(AllTagNames uiTag)
    {
        _landscapeCanvas.ShowRightTopSideUI(uiTag);
        _portraitCanvas.ShowRightTopSideUI(uiTag);
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
        _tyfpBackground.color = color;
    }

    public void ShowThankYouForPlayingUI()
    {
        _badAspectRatioCanvas.gameObject.SetActive(false);
            
        _landscapeCanvas.SetVisibility(false);
        _portraitCanvas.SetVisibility(false);

        _tyfpTotalTurnsText.text = $"{Creator.saveDataSo.totalTurns}";
        _tyfpTotalMovesText.text = $"{Creator.saveDataSo.totalMoves}";
        _tyfpTotalCapturesText.text = $"{Creator.saveDataSo.totalCaptures}";

        TimeSpan elapsed = TimeSpan.FromSeconds(Creator.saveDataSo.totalSeconds);

        string timeSpan = string.Format("{0:D2}h, {1:D2}m, {2:D2}s",
            (int)elapsed.TotalHours,
            elapsed.Minutes,
            elapsed.Seconds);
        _tyfpTotalTimeText.text = timeSpan;
        
        _tyfpCanvas.gameObject.SetActive(true);
        _tyfpTween.Enlarge();
        
        _canvasType = AllTagNames.ThankYouForPlaying;
    }
}
