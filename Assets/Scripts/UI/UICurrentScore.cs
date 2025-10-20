using TMPro;
using UnityEngine;

public class UICurrentScore : UIPanel
{
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    private TextMeshProUGUI _scoreText;
    
    private TextMeshProUGUI _star1ScoreRequiredText;
    private TextMeshProUGUI _star2ScoreRequiredText;
    private TextMeshProUGUI _star3ScoreRequiredText;

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _turnsText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsTurns);
        _movesText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsMoves);
        _scoreText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsScore);
        
        _star1ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star1ScoreRequired);
        _star2ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star2ScoreRequired);
        _star3ScoreRequiredText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Star3ScoreRequired);

    }

    public void SetNewLevel(Level level)
    {
        SetCurrentScoreText(1, 0);
        
        _star1ScoreRequiredText.text = $"{level.star1Score}";
        _star2ScoreRequiredText.text = $"{level.star2Score}";
        _star3ScoreRequiredText.text = $"{level.star3Score}";

    }

    public void SetCurrentScoreText(int turns, int moves)
    {
        _turnsText.text = $"{turns}";
        _movesText.text = $"{moves}";
        _scoreText.text = $"{turns*moves}";
    }
}
