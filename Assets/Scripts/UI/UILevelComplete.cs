using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelComplete : UIPanel
{
    private TurnSystem _turnSystem;
    
    private ButtonManager _nextLevelButton;

    private Image _starOneImage;
    private Image _starTwoImage;
    private Image _starThreeImage;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    private TextMeshProUGUI _scoreText;
    
    private TextMeshProUGUI _star1ScoreRequiredText;
    private TextMeshProUGUI _star2ScoreRequiredText;
    private TextMeshProUGUI _star3ScoreRequiredText;

    private ParticleSystem _confetti;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _confetti = Creator.GetFirstObjectWithName(AllTagNames.Confetti).GetComponent<ParticleSystem>();
        
        _turnsText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsTurns);
        _movesText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsMoves);
        _scoreText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsScore);
        
        _starOneImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star1Image);
        _starTwoImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star2Image);
        _starThreeImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star3Image);

        _star1ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star1ScoreRequired);
        _star2ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star2ScoreRequired);
        _star3ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star3ScoreRequired);
        
        _nextLevelButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonNextLevel);
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();
            
            Level nextLevel = Creator.levelsSo.GetLevel(_turnSystem.currentLevel.section, _turnSystem.currentLevel.level + 1);
            _turnSystem.LoadLevelRuntime(nextLevel);
        });

        ButtonManager levelSelect = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLevels);
        levelSelect.onClick.AddListener(() =>
        {
            _audioSystem.PlayMenuOpenSfx();
            
            List<UILevels> uiLevelss = _uiSystem.GetUI<UILevels>();
            foreach (UILevels uiLevels in uiLevelss)
            {
                uiLevels.SetLevels(_turnSystem.currentLevel.section);
            }
            
            _uiSystem.ShowRightTopSideUI(AllTagNames.UILevels);
            
            _uiSystem.ShowLeftBotSideUI(AllTagNames.UISections);
        });
    }

    public override void Show()
    {
        Level levelCompleted = _turnSystem.currentLevel;
        _turnsText.text = $"{Creator.statsTurns}";
        _movesText.text = $"{Creator.statsMoves}";
        int score = Creator.statsTurns * Creator.statsMoves;
        _scoreText.text = $"{score}";

        _star1ScoreRequiredText.text = $"{levelCompleted.star1Score}";
        _star2ScoreRequiredText.text = $"{levelCompleted.star2Score}";
        _star3ScoreRequiredText.text = $"{levelCompleted.star3Score}";

        SaveLatestLevelScore(levelCompleted, score);
        
        List<UISections> uiSectionss = _uiSystem.GetUI<UISections>();
        foreach (UISections uiSections in uiSectionss)
        {
            uiSections.UpdateStarCount();
        }
        
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

        int emissionRate = 0;
        if (threeStar)
        {
            emissionRate = Creator.boardSo.threeStarConfettiEmissionRate;
        }
        else if (twoStar)
        {
            emissionRate = Creator.boardSo.twoStarConfettiEmissionRate;
        }
        else if (oneStar)
        {
            emissionRate = Creator.boardSo.oneStarConfettiEmissionRate;
        }

        var emission = _confetti.emission;
        emission.rateOverTime = emissionRate;
        
        _confetti.Play();
        
        _panel.gameObject.SetActive(true);

        if (oneStar || twoStar || threeStar)
        {
            _audioSystem.PlayLevelCompleteSfx();
        }
        else
        {
            _audioSystem.PlayerGameOverSfx();
        }
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
