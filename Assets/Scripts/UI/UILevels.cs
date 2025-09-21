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
        public Level level;
    }

    private ButtonManager _playButton;
    
    private TextMeshProUGUI _sectionText;
    private TextMeshProUGUI _levelText;

    private Level _levelToLoad;
    
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
            _levelInfos[i].level = sectionData.levels[i];
        }
    }

    private void LoadLevel()
    {
        _turnSystem.LoadLevelRuntime(_levelToLoad);
        
        _uiSystem.HideLeftSideUI();
        _uiSystem.ShowRightSideUI(AllTagNames.UIChain);
    }
}
