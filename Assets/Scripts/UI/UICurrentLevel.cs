using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICurrentLevel : UIPanel
{
    private TurnSystem _turnSystem;
    
    private ButtonManager _leftButton;
    private ButtonManager _rightButton;
    private ButtonManager _sectionsButton;

    private TextMeshProUGUI _currentLevelText;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    private TextMeshProUGUI _scoreText;
    
    private TextMeshProUGUI _star1ScoreRequiredText;
    private TextMeshProUGUI _star2ScoreRequiredText;
    private TextMeshProUGUI _star3ScoreRequiredText;
    
    private Image _star1Image;
    private Image _star2Image;
    private Image _star3Image;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _leftButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLeft);
        _rightButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonRight);
        _sectionsButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonBack);
        
        _currentLevelText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.LevelText);
        
        _turnsText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsTurns);
        _movesText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsMoves);
        _scoreText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsScore);
        
        _star1ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star1ScoreRequired);
        _star2ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star2ScoreRequired);
        _star3ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star3ScoreRequired);

        _star1Image = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star1Image);
        _star2Image = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star2Image);
        _star3Image = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star3Image);
        
        _leftButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();
            
            Level nextLevel;
            Level currentLevel = _turnSystem.currentLevel;
            if (currentLevel is { section: 1, level: 1 })
            {
                SectionData latestSectionUnlocked = LatestSectionUnlocked();
                nextLevel = latestSectionUnlocked.levels[^1];
            }
            else if (currentLevel.level == 1)
            {
                SectionData nextSectionData = Creator.levelsSo.GetSection(currentLevel.section - 1);
                nextLevel = nextSectionData.levels[^1];
            }
            else
            {
                nextLevel = Creator.levelsSo.GetLevel(currentLevel.section, currentLevel.level - 1);
            }
            
            _turnSystem.LoadLevelRuntime(nextLevel);
        });
        
        _rightButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();
            
            /*
             * If the current level.isLastInSection and current section is latest unlocked section, load section 1 level 1
             * Else If the current level.isLastInSection, load section {X + 1} level 1
             * Else load section X level X + 1
             */
            
            SectionData latestSectionUnlocked = LatestSectionUnlocked();
            
            Level nextLevel;
            Level currentLevel = _turnSystem.currentLevel;

            if(currentLevel.isLastInSection && latestSectionUnlocked.section == currentLevel.section)
            {
                nextLevel = Creator.levelsSo.GetLevel(1,1);
            }
            else if (currentLevel.isLastInSection)
            {
                SectionData nextSectionData = Creator.levelsSo.GetSection(currentLevel.section + 1);
                nextLevel = nextSectionData.levels[0];
            }
            else
            {
                nextLevel = Creator.levelsSo.GetLevel(currentLevel.section, currentLevel.level + 1);
            }
            
            _turnSystem.LoadLevelRuntime(nextLevel);
        });
        
        _sectionsButton.onClick.AddListener(() =>
        {
            _uiSystem.ShowLeftBotSideUI(AllTagNames.UISections);
            
            _audioSystem.PlayMenuOpenSfx();
        });
    }

    public void SetNewLevel(Level level)
    {
        SetCurrentScoreText(1, 0);
        SetStarsScored(level);
        
        _star1ScoreRequiredText.text = $"{level.star1Score}";
        _star2ScoreRequiredText.text = $"{level.star2Score}";
        _star3ScoreRequiredText.text = $"{level.star3Score}";

        _currentLevelText.text = $"{level.section} - {level.level}";
    }

    public void SetStarsScored(Level level)
    {
        foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
        {
            if (levelSaveData.section == level.section && levelSaveData.level == level.level)
            {
                _star1Image.sprite = levelSaveData.starsScored >= 1 ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                _star1Image.color = levelSaveData.starsScored >= 1 ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
                _star2Image.sprite = levelSaveData.starsScored >= 2 ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                _star2Image.color = levelSaveData.starsScored >= 2 ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

                _star3Image.sprite = levelSaveData.starsScored == 3 ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                _star3Image.color = levelSaveData.starsScored == 3 ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

                return;
            }
        }
        
        _star1Image.sprite = Creator.levelCompleteSo.starHollowSprite;
        _star1Image.color = Creator.levelCompleteSo.starHollowColor;
        
        _star2Image.sprite = Creator.levelCompleteSo.starHollowSprite;
        _star2Image.color = Creator.levelCompleteSo.starHollowColor;

        _star3Image.sprite = Creator.levelCompleteSo.starHollowSprite;
        _star3Image.color = Creator.levelCompleteSo.starHollowColor;

    }
    
    public void SetCurrentScoreText(int turns, int moves)
    {
        _turnsText.text = $"{turns}";
        _movesText.text = $"{moves}";
        _scoreText.text = $"{turns*moves}";
    }
    
    private SectionData LatestSectionUnlocked()
    {
        int starsScored = 0;
        foreach (LevelSaveData level in Creator.saveDataSo.levels)
        {
            starsScored += level.starsScored;
        }
            
        SectionData latestSectionUnlocked = Creator.levelsSo.sections[^1];
        for (int i = 0; i < Creator.levelsSo.sections.Count; i++)
        {
            if (Creator.levelsSo.sections[i].starsRequiredToUnlock > starsScored)
            {
                latestSectionUnlocked = Creator.levelsSo.sections[i - 1];
                break;
            }
        }
        
        return latestSectionUnlocked;
    }
}
