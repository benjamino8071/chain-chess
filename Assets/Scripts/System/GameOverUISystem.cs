using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    public bool IsShowing => _gameOverUI.gameObject.activeSelf;
    
    private Transform _gameOverUI;

    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _quoteText;
    private TextMeshProUGUI _authorText;
    //private TextMeshProUGUI _reasonText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();

        _gameOverUI = creator.GetFirstObjectWithName(AllTagNames.GameOver).transform;
        
        Transform resetButtonTf = creator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Retry);
        Button resetButton = resetButtonTf.GetComponent<Button>();
        resetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = creator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("Main_Menu_Scene");
        });
        
        _titleText = creator.GetChildComponentByName<TextMeshProUGUI>(_gameOverUI.gameObject, AllTagNames.TitleText);
        _quoteText = creator.GetChildComponentByName<TextMeshProUGUI>(_gameOverUI.gameObject, AllTagNames.QuoteText);
        _authorText = creator.GetChildComponentByName<TextMeshProUGUI>(_gameOverUI.gameObject, AllTagNames.AuthorText);

        Hide();
    }

    public void Show(GameOverReason gameOverReason)
    {
        Quote quote = Creator.gameOverSo.GetRandomQuote();
        
        switch (gameOverReason)
        {
            case GameOverReason.Captured:
            {
                _titleText.text = "Captured";
                break;
            }
            case GameOverReason.NoTurns:
            {
                _titleText.text = "Ran out of turns";
                break;
            }
            case GameOverReason.Locked:
            {
                _titleText.text = "Locked";
                break;
            }
        }

        _quoteText.text = '"'+quote.quote+'"';
        _authorText.text = "- "+quote.name;
        
        _gameOverUI.gameObject.SetActive(true);
        _audioSystem.PlayerGameOverSfx();
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
