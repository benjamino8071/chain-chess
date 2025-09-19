using System.Collections.Generic;
using Michsky.MUIP;
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
        public GameObject go;
        public RectTransform rect;
        public Level level;
        public ButtonManager levelButton;
        public Image starOneImage;
        public Image starTwoImage;
        public Image starThreeImage;
    }
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _levelInfos = new(Creator.levelsSo.AllLevels().Count);

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
        
        Transform guiBottom = Creator.GetFirstObjectWithName(AllTagNames.UILevels);
        
        Hide();
    }
}
