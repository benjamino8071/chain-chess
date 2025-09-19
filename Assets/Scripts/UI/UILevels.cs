using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
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
        public int levelNumber;
    }

    private TextMeshProUGUI _sectionText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _levelInfos = new(10);
        
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
            levelInfo.starOneImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starTwoImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starThreeImage.sprite = Creator.levelCompleteSo.starHollowSprite;

            _levelInfos.Add(levelInfo);
        }
        
        float yPosition = 400 + Creator.levelSelectSo.gap;
        
        /*foreach (Level level in Creator.levelsSo.AllLevels())
        {
            GameObject levelInfoGo = Creator.InstantiateGameObjectWithParent(Creator.levelInfoPrefab, _pivot);
            
            levelInfoGo.name = $"Level {level.level} Info";
            
            LevelInfo levelInfo = new()
            {
                go = levelInfoGo,
                rect = levelInfoGo.GetComponent<RectTransform>(),
                level = level,
                levelButton = Creator.GetChildComponentByName<ButtonManager>(levelInfoGo, AllTagNames.ButtonLevels),
                starOneImage = Creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star1Image),
                starTwoImage = Creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star2Image),
                starThreeImage = Creator.GetChildComponentByName<Image>(levelInfoGo, AllTagNames.Star3Image)
            };
            
            yPosition -= Creator.levelSelectSo.gap;
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
        }*/

        _sectionText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.SectionText);
        
        Hide();
    }

    public void SetLevels(int section)
    {
        _sectionText.text = $"Section {section}";
        
        SectionData sectionData = Creator.levelsSo.GetSection(section);

        for (int i = 0; i < sectionData.levels.Count; i++)
        {
            Level level = sectionData.levels[i];
            _levelInfos[i].levelNumber = level.level;
            _levelInfos[i].levelButton.SetText($"{level.level}");
        }
    }
}
