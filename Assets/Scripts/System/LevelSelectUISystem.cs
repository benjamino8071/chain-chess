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
    private TurnSystem _turnSystem;
    
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
    
    private float3 _desiredPivotLocalPosition;

    private float _pivotLowerBoundY;
    private float _pivotUpperBoundY;
    private float _mouseScreenPositionYLastFrame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        
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
                levelButton = creator.GetChildComponentByName<ButtonManager>(levelInfoGo, AllTagNames.ButtonLevels),
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
                
                Hide();
            });
            
            _levelInfos.Add(levelInfo);
        }

        _pivotLowerBoundY = 50;
        _pivotUpperBoundY = -(yPosition + 400);
        
        float startingYPos = _pivotLowerBoundY + ((creator.levelsSo.levelOnLoad - 1) * 200);
        startingYPos = math.clamp(startingYPos, _pivotLowerBoundY, _pivotUpperBoundY);
        _pivot.localPosition = new(0, startingYPos);
        _desiredPivotLocalPosition = _pivot.localPosition;
        
        Transform guiBottom = creator.GetFirstObjectWithName(AllTagNames.UILevels);
        
        _levelSelectButton = creator.GetChildComponentByName<ButtonManager>(guiBottom.gameObject, AllTagNames.ButtonLevels);
        _levelSelectButton.onClick.AddListener(() =>
        {
            if (!_levelSelectUI.gameObject.activeSelf)
            {
                Show();
                _audioSystem.PlayPauseOpenSfx();
            }
            else
            {
                Hide();
                
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
