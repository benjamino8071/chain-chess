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

    private TextMeshProUGUI _reasonText;
    
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

        Transform reasonTextTf = creator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Text);
        _reasonText = reasonTextTf.GetComponent<TextMeshProUGUI>();

        Hide();
    }

    public void Show(GameOverReason gameOverReason)
    {
        switch (gameOverReason)
        {
            case GameOverReason.Captured:
            {
                _reasonText.text = "Piece captured";
                break;
            }
            case GameOverReason.NoTurns:
            {
                _reasonText.text = "Ran out of turns";
                break;
            }
            case GameOverReason.Locked:
            {
                _reasonText.text = "Piece locked";
                break;
            }
        }
        
        _gameOverUI.gameObject.SetActive(true);
        _audioSystem.PlayerGameOverSfx();
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
