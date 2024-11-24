using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUISystem : Dependency
{
    private Transform _endGameGUI;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _endGameGUI = GameObject.FindWithTag("EndGame").transform;

        Button tryAgainButton = _endGameGUI.GetComponentInChildren<Button>();
        tryAgainButton.onClick.AddListener(TryAgain);
        
        Hide();
    }

    private void TryAgain()
    {
        _creator.playerSystemSo.startingPiece = Piece.NotChosen;
        _creator.playerSystemSo.roomNumberSaved = 0;
        _creator.timerSo.currentTime = _creator.timerSo.maxTime;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Show()
    {
        _endGameGUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _endGameGUI.gameObject.SetActive(false);
    }
}
