using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : UIPanel
{
    private TurnSystem _turnSystem;
    
    private TextMeshProUGUI _quoteText;
    private TextMeshProUGUI _reasonText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        ButtonManager tryAgainButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonReset);
        tryAgainButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            _turnSystem.ReloadCurrentLevel();
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

        _reasonText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.ReasonText);
        _quoteText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.QuoteText);
    }
    
    public void SetUI(GameOverReason gameOverReason)
    {        
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
        
        Quote quote = Creator.gameOverSo.GetRandomQuote();
        _quoteText.text = '"'+quote.quote+'"'+"\n\n<b>"+quote.name+"</b>";
        
        _panel.gameObject.SetActive(true);

        if (_uiSystem.canvasType == _parentCanvas.canvasType)
        {
            _audioSystem.PlayerGameOverSfx();
        }
    }
}
