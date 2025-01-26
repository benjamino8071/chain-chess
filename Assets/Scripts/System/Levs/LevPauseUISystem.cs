using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevPauseUISystem : LevDependency
{
    private LevPlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _playerSystem = levCreator.GetDependency<LevPlayerSystem>();
        
        _pauseGUI = levCreator.GetFirstObjectWithName(AllTagNames.Pause).transform;
        
        _pauseButton = levCreator.GetFirstObjectWithName(AllTagNames.PauseButton).GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetStateForAllPlayers(LevPlayerController.States.WaitingForTurn);
            }
            else
            {
                Hide();
                _playerSystem.SetStateForAllPlayers(LevPlayerController.States.Idle);
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
        
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf);
    }

    public void Show()
    {
        _pauseGUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);
    }
}
