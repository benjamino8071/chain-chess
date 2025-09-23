using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelComplete : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;
    
    private ButtonManager _nextLevelButton;

    private Image _starOneImage;
    private Image _starTwoImage;
    private Image _starThreeImage;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _turnsText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsTurns);
        _movesText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsMoves);
        
        _starOneImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star1Image);
        _starTwoImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star2Image);
        _starThreeImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star3Image);
        
        _nextLevelButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonNextLevel);
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Level nextLevel = Creator.levelsSo.GetLevel(_turnSystem.currentLevel.section, _turnSystem.currentLevel.level + 1);
            _turnSystem.LoadLevelRuntime(nextLevel);
        });

        ButtonManager levelSelect = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLevels);
        levelSelect.onClick.AddListener(() =>
        {
            UILevels uiLevels = _uiSystem.GetUI<UILevels>();
            uiLevels.SetLevels(_turnSystem.currentLevel.section);
            
            _uiSystem.ShowRightSideUI(AllTagNames.UILevels);
            _uiSystem.ShowLeftSideUI(AllTagNames.UISections);
        });
    }

    public override void Show()
    {
        Level levelCompleted = _turnSystem.currentLevel;
        _turnsText.text = $"{Creator.statsTurns}";
        _movesText.text = $"{Creator.statsMoves}";
        int score = Creator.statsTurns * Creator.statsMoves;
        
        SaveLatestLevelScore(levelCompleted, score);
        UISections uiSections = _uiSystem.GetUI<UISections>();
        uiSections.UpdateStarCount();
        
        Debug.Log($"Score for level {levelCompleted.level}: {score}");
        
        bool oneStar = score <= levelCompleted.star1Score;
        bool twoStar = score <= levelCompleted.star2Score;
        bool threeStar = score <= levelCompleted.star3Score;
        
        _starOneImage.sprite = oneStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starOneImage.color = oneStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
        _starTwoImage.sprite = twoStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starTwoImage.color = twoStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

        _starThreeImage.sprite = threeStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starThreeImage.color = threeStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
        _nextLevelButton.gameObject.SetActive(!levelCompleted.isLastInSection);
        
        _panel.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }
    
    private void SaveLatestLevelScore(Level level, int score)
    {
        bool found = false;
        int starsScored = 0;
        starsScored += score <= level.star1Score ? 1 : 0;
        starsScored += score <= level.star2Score ? 1 : 0;
        starsScored += score <= level.star3Score ? 1 : 0;
        
        for (int i = 0; i < Creator.saveDataSo.levels.Count; i++)
        {
            LevelSaveData levelSaveData = Creator.saveDataSo.levels[i];
            
            if (levelSaveData.section != level.section || levelSaveData.level != level.level)
            {
                continue;
            }

            if (score < levelSaveData.score)
            {
                levelSaveData.starsScored = starsScored;
                levelSaveData.score = score;

                Creator.saveDataSo.levels[i] = levelSaveData;
            }
            
            found = true;
            break;
        }

        if (!found)
        {
            Creator.saveDataSo.levels.Add(new()
            {
                section = level.section,
                level = level.level,
                score = score,
                starsScored = starsScored,
            });
        }
        
        Creator.SaveToDisk();
    }
}
