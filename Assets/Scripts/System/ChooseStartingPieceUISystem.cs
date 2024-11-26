using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChooseStartingPieceUISystem : ElDependency
{
    private PlayerSystem _playerSystem;
    private TurnInfoUISystem _turnInfoUISystem;
    private TimerUISystem _timerUISystem;
    private CapturedPiecesUISystem _capturedPiecesUISystem;
    
    private Transform _chooseStartingPiece;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (Creator.TryGetDependency("TurnInfoUISystem", out TurnInfoUISystem turnInfoUISystem))
        {
            _turnInfoUISystem = turnInfoUISystem;
        }
        if (Creator.TryGetDependency("TimerUISystem", out TimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.TryGetDependency("CapturedPiecesUISystem", out CapturedPiecesUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        
        _chooseStartingPiece = GameObject.FindWithTag("ChooseStartingPiece").transform;
        
        if (Creator.playerSystemSo.startingPiece != Piece.NotChosen && Creator.playerSystemSo.roomNumberSaved > 0)
        {
            //The player is continuing from the latest room. Just set piece straight away
            _chooseStartingPiece.gameObject.SetActive(false);
            ShowNewPiece();
            
            Creator.StartACoRoutine(WaitToShowPiece());
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
        
        PieceChosen(Creator.playerSystemSo.startingPiece);
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
        _capturedPiecesUISystem.ShowNewPiece(Creator.playerSystemSo.startingPiece, true);
    }
}
