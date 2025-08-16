using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private BoardSystem _boardSystem;
    private SettingsUISystem _settingsUISystem;
    private TurnSystem _turnSystem;
    private LevelCompleteUISystem _levelCompleteUISystem;
    private GameOverUISystem _gameOverUISystem;
    
    public bool IsShowing => _levelSelectUI.gameObject.activeSelf;
    
    private Transform _levelSelectUI;
    
    private ButtonManager _levelSelectButton;
    
    private List<LevelInfo> _levelInfos;
    private class LevelInfo
    {
        public GameObject go;
        public RectTransform rect;
        public Level level;
        public ButtonManager levelButton;
        public Image starOneImage;
        public Image starTwoImage;
        public Image starThreeImage;
    }
    
    private Transform _pivot;
    
    private PieceController _pieceControllerSelected;
    
    private float3 _desiredPivotLocalPosition;

    private float _pivotLowerBoundY;
    private float _pivotUpperBoundY;
    private float _mouseScreenPositionYLastFrame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _levelCompleteUISystem = creator.GetDependency<LevelCompleteUISystem>();
        _gameOverUISystem = creator.GetDependency<GameOverUISystem>();
        
        _levelSelectUI = creator.GetFirstObjectWithName(AllTagNames.LevelSelect);

        _pivot = creator.GetChildComponentByName<Transform>(_levelSelectUI.gameObject, AllTagNames.Pivot);
        
        _levelInfos = new(creator.levelsSo.AllLevels().Count);

        float yPosition = 400 + creator.levelSelectSo.gap;
        
        foreach (Level level in creator.levelsSo.AllLevels())
        {
            GameObject levelInfoGo = creator.InstantiateGameObjectWithParent(creator.levelInfoPrefab, _pivot);
            
            levelInfoGo.name = $"Level {level.level} Info";
            
            LevelInfo levelInfo = new()
            {
                go = levelInfoGo,
                rect = levelInfoGo.GetComponent<RectTransform>(),
                level = level,
                levelButton = creator.GetChildComponentByName<ButtonManager>(levelInfoGo, AllTagNames.LevelButton),
                starOneImage = creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star1Image),
                starTwoImage = creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star2Image),
                starThreeImage = creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star3Image)
            };
            
            yPosition -= creator.levelSelectSo.gap;
            levelInfo.rect.anchoredPosition = new(0, yPosition);

            levelInfo.levelButton.SetText($"Level {level.level}");
            levelInfo.levelButton.onClick.AddListener(() =>
            {
                Creator.levelsSo.levelOnLoad = levelInfo.level.level;
                _turnSystem.LoadLevelRuntime();
                _levelCompleteUISystem.Hide();
                _gameOverUISystem.Hide();
                
                Hide();
            });
            
            _levelInfos.Add(levelInfo);
        }

        _pivotLowerBoundY = 50;
        _pivot.localPosition = new(0, _pivotLowerBoundY);
        _desiredPivotLocalPosition = _pivot.localPosition;
        _pivotUpperBoundY = -(yPosition + 400);
        
        Transform guiBottom = creator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        
        _levelSelectButton = creator.GetChildComponentByName<ButtonManager>(guiBottom.gameObject, AllTagNames.LevelButton);
        _levelSelectButton.onClick.AddListener(() =>
        {
            _settingsUISystem.Hide();
            
            if (!_levelSelectUI.gameObject.activeSelf)
            {
                if (_boardSystem.activeSideSystem.pieceControllerSelected is { } pieceControllerSelected)
                {
                    _pieceControllerSelected = pieceControllerSelected;
                    pieceControllerSelected.SetState(PieceController.States.Paused);
                }
                Show();
                _audioSystem.PlayPauseOpenSfx();
            }
            else
            {
                Hide();
                
                if (_pieceControllerSelected != null)
                {
                    _pieceControllerSelected.SetState(PieceController.States.FindingMove);
                    _pieceControllerSelected = null;
                }
                _audioSystem.PlayPauseCloseSfx();
            }
        });
        
        UpdateLevelText(creator.levelsSo.levelOnLoad);
        
        Hide();
    }

    public override void GameUpdate(float dt)
    {
        if (!_levelSelectUI.gameObject.activeSelf)
        {
            return;
        }
        
        float3 mouseScreenPosition = _boardSystem.GetFloatPointOnBoard();
        float mouseScreenPositionY = mouseScreenPosition.y;
        
        if (Creator.inputSo.leftMouseButton.action.IsPressed() && mouseScreenPositionY >= Creator.boardSo.minY && mouseScreenPositionY < Creator.boardSo.maxY)
        {
            mouseScreenPositionY *= Creator.levelSelectSo.swipeSpeed;
            
            if (Creator.inputSo.leftMouseButton.action.WasPressedThisFrame())
            {
                _mouseScreenPositionYLastFrame = mouseScreenPositionY;
            }

            UpdatePivotLocalPosition(mouseScreenPositionY, _mouseScreenPositionYLastFrame);
            
            _mouseScreenPositionYLastFrame = mouseScreenPositionY;
        }
        else if (Creator.inputSo.scrollWheel.action.WasPerformedThisFrame())
        {
            float2 scrollWheel = Creator.inputSo.scrollWheel.action.ReadValue<Vector2>();
            int scrollWheelY = (int)scrollWheel.y;
            
            UpdatePivotLocalPosition(Creator.inputSo.scrollPositionChange * -scrollWheelY, 0);
        }
        
        Vector3 lerpPos = math.lerp(_pivot.localPosition, _desiredPivotLocalPosition, dt * Creator.levelSelectSo.scrollSpeed);
        _pivot.localPosition = lerpPos;
    }

    private void UpdatePivotLocalPosition(float currentFrameY, float lastFrameY)
    {
        float mousePosYChange = (currentFrameY - lastFrameY);
        float3 pivotLocalPosition = _desiredPivotLocalPosition;
        float pivotNewLocalY = pivotLocalPosition.y + mousePosYChange;

        pivotLocalPosition.y = math.clamp(pivotNewLocalY, _pivotLowerBoundY, _pivotUpperBoundY);
        _desiredPivotLocalPosition = pivotLocalPosition;
    }

    public void UpdateLevelText(int level)
    {
        _levelSelectButton.SetText($"Level {level}");
    }

    public void Show()
    {
        _levelSelectUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _levelSelectUI.gameObject.SetActive(false);
    }
}
