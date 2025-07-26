using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    public bool isShowing => _gameOverUI.gameObject.activeSelf;
    
    private Transform _gameOverUI;
    
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

        Hide();
    }

    public void Show()
    {
        _gameOverUI.gameObject.SetActive(true);
        _audioSystem.PlayerGameOverSfx();
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
