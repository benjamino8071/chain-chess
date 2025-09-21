using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;
    
    private TextMeshProUGUI _quoteText;
    private TextMeshProUGUI _reasonText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        ButtonManager tryAgainButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonReset);
        tryAgainButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.ReloadCurrentLevel();
        });
        
        ButtonManager levelSelect = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLevels);
        levelSelect.onClick.AddListener(() =>
        {
            UILevels uiLevels = _uiSystem.GetUI<UILevels>();
            uiLevels.SetLevels(_turnSystem.currentLevel.section);
            
            _uiSystem.ShowRightSideUI(AllTagNames.UILevels);
        });

        _reasonText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.ReasonText);
        _quoteText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.QuoteText);
        
        Hide();
    }

    public void SetUI(GameOverReason gameOverReason)
    {
        Quote quote = Creator.gameOverSo.GetRandomQuote();
        
        switch (gameOverReason)
        {
            case GameOverReason.Captured:
            {
                _reasonText.text = "Captured";
                break;
            }
            case GameOverReason.NoTurns:
            {
                _reasonText.text = "Ran out of turns";
                break;
            }
            case GameOverReason.Locked:
            {
                _reasonText.text = "Blocked";
                break;
            }
        }

        _quoteText.text = '"'+quote.quote+'"'+"\n\n<b>"+quote.name+"</b>";
    }
    
    public override void Show()
    {
        base.Show();
        
        _audioSystem.PlayerGameOverSfx();
    }
}
