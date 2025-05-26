using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevPauseUISystem : LevDependency
{
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevGameOverUISystem _gameOverUISystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();

        _pauseGUI = levCreator.GetFirstObjectWithName(AllTagNames.Pause).transform;
        
        _pauseButton = levCreator.GetFirstObjectWithName(AllTagNames.PauseButton).GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf && !_levelCompleteUISystem.isShowing && !_gameOverUISystem.isShowing)
            {
                _pauseGUI.gameObject.SetActive(true);
            }
            else
            {
                _pauseGUI.gameObject.SetActive(false);
            }
        });

        Transform resetButtonTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.ResetButton);
        Button resetButton = resetButtonTf.GetComponent<Button>();
        resetButton.onClick.AddListener(() =>
        {
            Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });
        
        _pauseGUI.gameObject.SetActive(false);
    }

    public void ShowButton()
    {
        _pauseButton.gameObject.SetActive(true);
    }

    public void HideButton()
    {
        _pauseButton.gameObject.SetActive(false);
    }
}
