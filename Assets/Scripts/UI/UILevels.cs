using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILevels : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;
    
    private List<LevelInfo> _levelInfos;
    private class LevelInfo
    {
        public ButtonManager levelButton;
        public Image starOneImage;
        public Image starTwoImage;
        public Image starThreeImage;
        public Level level;
    }

    private ButtonManager _playButton;
    
    private TextMeshProUGUI _sectionText;
    private TextMeshProUGUI _levelText;
    
    private GameObject _levelScrollWheel;

    private Transform _pivot;
    
    private Level _levelToLoad;
    
    private float _mousePosYLastFrame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _levelScrollWheel = Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.HoverOverPanel).gameObject;
        _pivot = Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.Pivot);
        
        _levelInfos = new(10);
        
        _sectionText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.SectionText);
        _levelText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.LevelText);
        
        _playButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonPlay);
        _playButton.onClick.AddListener(LoadLevel);
        
        LevelButtons buttons = _panel.GetComponent<LevelButtons>();
        foreach (LevelButton levelButton in buttons.buttons)
        {
            LevelInfo levelInfo = new()
            {
                levelButton = levelButton.button,
                starOneImage = levelButton.starOneImage,
                starTwoImage = levelButton.starTwoImage,
                starThreeImage = levelButton.starThreeImage
            };
            
            levelInfo.levelButton.onClick.AddListener(() =>
            {
                _levelToLoad = levelInfo.level;
                _levelText.text = $"Level {_levelToLoad.section} - {_levelToLoad.level}";
                _levelText.gameObject.SetActive(true);
                _playButton.gameObject.SetActive(true);
            });
            
            levelInfo.starOneImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starTwoImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starThreeImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            
            _levelInfos.Add(levelInfo);
        }
        
        Hide();
        
        _mousePosYLastFrame = Input.mousePosition.y;
    }

    public override void GameUpdate(float dt)
    {
        bool overScrollWheel = false;
        
        List<RaycastResult> objectsUnderMouse = _uiSystem.objectsUnderMouse;
        foreach (RaycastResult objectUnderMouse in objectsUnderMouse)
        {
            if (objectUnderMouse.gameObject == _levelScrollWheel.gameObject)
            {
                overScrollWheel = true;
                break;
            }
        }

        float3 mousePos = Input.mousePosition;
        Vector2 scrollWheelValue = Creator.inputSo.scrollWheel.action.ReadValue<Vector2>();
        
        if (overScrollWheel && Creator.inputSo.leftMouseButton.action.IsPressed())
        {
            float mousePosYChange = mousePos.y - _mousePosYLastFrame;
            float3 chainParentLocalPos = _pivot.localPosition;
            chainParentLocalPos.y += mousePosYChange;
            
            chainParentLocalPos.y = math.clamp(chainParentLocalPos.y, 0, 420);
            
            _pivot.localPosition = chainParentLocalPos;
        }
        else if (overScrollWheel && scrollWheelValue.y != 0)
        {
            float positionChange = -scrollWheelValue.y * Creator.inputSo.scrollPositionChange;
            float3 chainParentLocalPos = _pivot.localPosition;
            chainParentLocalPos.y += positionChange;
            chainParentLocalPos.y = math.clamp(chainParentLocalPos.y, 0, 420);
            _pivot.localPosition = chainParentLocalPos;
        }
        
        _mousePosYLastFrame = mousePos.y;
    }

    public override void Show()
    {
        _playButton.gameObject.SetActive(false);
        _levelText.gameObject.SetActive(false);
        
        base.Show();
    }

    public void SetLevels(int section)
    {
        _sectionText.text = $"Section {section}";
        
        SectionData sectionData = Creator.levelsSo.GetSection(section);
        
        for (int i = 0; i < sectionData.levels.Count; i++)
        {
            if (i > sectionData.levels.Count - 1)
            {
                _levelInfos[i].levelButton.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                _levelInfos[i].level = sectionData.levels[i];
                _levelInfos[i].levelButton.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    private void LoadLevel()
    {
        _turnSystem.LoadLevelRuntime(_levelToLoad);
        
        _uiSystem.HideLeftSideUI();
        _uiSystem.ShowRightSideUI(AllTagNames.UIChain);
    }
}
