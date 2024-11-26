using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUISystem : ElDependency
{
    private Transform _gameOverGUI;

    private Button _continueButton;
    private Button _giveUpButton;

    private Button _tryAgainButton;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _gameOverGUI = GameObject.FindWithTag("GameOver").transform;

        _continueButton = GameObject.FindWithTag("Continue").GetComponent<Button>();
        _continueButton.onClick.AddListener(ContinueFromRoomX);
        _giveUpButton = GameObject.FindWithTag("GiveUp").GetComponent<Button>();
        _giveUpButton.onClick.AddListener(GiveUp);
        _tryAgainButton = GameObject.FindWithTag("TryAgain").GetComponent<Button>();
        _tryAgainButton.onClick.AddListener(GiveUp);
        
        _continueButton.gameObject.SetActive(false);
        _giveUpButton.gameObject.SetActive(false);
            
        _tryAgainButton.gameObject.SetActive(false);
        
        HideGameOver();
    }

    private void ContinueFromRoomX()
    {
        Creator.timerSo.currentTime -= Creator.timerSo.contFromRoomPenalty;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GiveUp()
    {
        Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        Creator.playerSystemSo.roomNumberSaved = 0;
        Creator.timerSo.currentTime = Creator.timerSo.maxTime;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowGameOver()
    {
        //Player has died in room 0 (silly player) - just restart the game
        if (Creator.playerSystemSo.roomNumberSaved == 0 || Creator.timerSo.currentTime <= Creator.timerSo.contFromRoomPenalty)
        {
            _continueButton.gameObject.SetActive(false);
            _giveUpButton.gameObject.SetActive(false);
            
            _tryAgainButton.gameObject.SetActive(true);
        }
        else
        {
            _tryAgainButton.gameObject.SetActive(false);
            
            _continueButton.gameObject.SetActive(true);
            _giveUpButton.gameObject.SetActive(true);

        }
        _gameOverGUI.gameObject.SetActive(true);
    }

    public void HideGameOver()
    {
        _gameOverGUI.gameObject.SetActive(false);
    }
}
