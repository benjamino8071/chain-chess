using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChooseStartingPieceUISystem : Dependency
{
    private PlayerSystem _playerSystem;
    private TurnInfoUISystem _turnInfoUISystem;
    private TimerUISystem _timerUISystem;
    private CapturedPiecesUISystem _capturedPiecesUISystem;
    
    private Transform _chooseStartingPiece;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        if (_creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (_creator.TryGetDependency("TurnInfoUISystem", out TurnInfoUISystem turnInfoUISystem))
        {
            _turnInfoUISystem = turnInfoUISystem;
        }
        if (_creator.TryGetDependency("TimerUISystem", out TimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (_creator.TryGetDependency("CapturedPiecesUISystem", out CapturedPiecesUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        
        _chooseStartingPiece = GameObject.FindWithTag("ChooseStartingPiece").transform;
        
        if (_creator.playerSystemSo.startingPiece != Piece.NotChosen && _creator.playerSystemSo.roomNumberSaved > 0)
        {
            //The player is continuing from the latest room. Just set piece straight away
            _chooseStartingPiece.gameObject.SetActive(false);
            ShowNewPiece();
            
            _creator.StartACoRoutine(WaitToShowPiece());
        }
        else
        {
            Button[] choosebuttons = _chooseStartingPiece.GetComponentsInChildren<Button>();
            foreach (Button choosebutton in choosebuttons)
            {
                switch (choosebutton.tag)
                {
                    case "ChoosePawn":
                        choosebutton.onClick.AddListener(ChoosePawn);
                        break;
                    case "ChooseRook":
                        choosebutton.onClick.AddListener(ChooseRook);
                        break;
                    case "ChooseKnight":
                        choosebutton.onClick.AddListener(ChooseKnight);
                        break;
                    case "ChooseBishop":
                        choosebutton.onClick.AddListener(ChooseBishop);
                        break;
                    case "ChooseKing":
                        choosebutton.onClick.AddListener(ChooseKing);
                        break;
                    case "ChooseQueen":
                        choosebutton.onClick.AddListener(ChooseQueen);
                        break;
                }
            }
        }
    }

    private IEnumerator WaitToShowPiece()
    {
        yield return new WaitForSeconds(1.7f);
        
        PieceChosen(_creator.playerSystemSo.startingPiece);
    }

    private void ChoosePawn()
    {
        PieceChosen(Piece.Pawn);
        ShowNewPiece();
    }

    private void ChooseRook()
    {
        PieceChosen(Piece.Rook);
        ShowNewPiece();
    }

    private void ChooseKnight()
    {
        PieceChosen(Piece.Knight);
        ShowNewPiece();
    }

    private void ChooseBishop()
    {
        PieceChosen(Piece.Bishop);
        ShowNewPiece();
    }

    private void ChooseKing()
    {
        PieceChosen(Piece.King);
        ShowNewPiece();
    }

    private void ChooseQueen()
    {
        PieceChosen(Piece.Queen);
        ShowNewPiece();
    }

    private void PieceChosen(Piece piece)
    {
        _playerSystem.SetDefaultPiece(piece);
        _turnInfoUISystem.SwitchTurn(TurnInfoUISystem.Turn.Player);
        _chooseStartingPiece.gameObject.SetActive(false);
        _timerUISystem.StartTimer();
    }

    private void ShowNewPiece()
    {
        _capturedPiecesUISystem.ShowNewPiece(_creator.playerSystemSo.startingPiece, true);
    }
}
