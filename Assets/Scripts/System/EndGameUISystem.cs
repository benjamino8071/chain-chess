using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUISystem : ElDependency
{
    private Transform _endGameGUI;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _endGameGUI = GameObject.FindWithTag("EndGame").transform;

        Button tryAgainButton = _endGameGUI.GetComponentInChildren<Button>();
        tryAgainButton.onClick.AddListener(TryAgain);
        
        Hide();
    }

    private void TryAgain()
    {
        Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        Creator.playerSystemSo.roomNumberSaved = 0;
        Creator.timerSo.currentTime = Creator.timerSo.maxTime;
        
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
