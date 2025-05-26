using System.Collections.Generic;
using UnityEngine;

public class LevWhiteSystem : LevDependency
{
    private LevValidMovesSystem _validMovesSystem;
    private LevBoardSystem _boardSystem;
    private LevTurnSystem _turnSystem;
    private LevChainUISystem _chainUISystem;
    private LevEndGameSystem _endGameSystem;
    private LevBlackSystem _blackSystem;
    
    private List<LevPieceController> _pieceControllers = new ();

    private LevPieceController _pieceControllerSelected;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _endGameSystem = levCreator.GetDependency<LevEndGameSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();

        List<Transform> whiteSps = levCreator.GetObjectsByName(AllTagNames.WhiteSp);
        foreach (Transform whiteSp in whiteSps)
        {
            PieceType pieceType = whiteSp.GetComponent<PieceType>();
            
            LevPieceController levPlayerController = Creator.whiteControlledBy == ControlledBy.Player 
                ? new LevPlayerController() : new LevAIController();
            levPlayerController.GameStart(levCreator);
            levPlayerController.Init(whiteSp, pieceType.piece, LevPieceController.PieceColour.White);
            
            _pieceControllers.Add(levPlayerController);
        }
    }

    public override void GameUpdate(float dt)
    {
        if (_turnSystem.CurrentTurn() == LevTurnSystem.Turn.Black)
        {
            return;
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
        
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame() && Creator.whiteControlledBy == ControlledBy.Player)
        {
            /*
             * If the player selects another piece when they haven't made a move
             * then we want to select that piece
             */
            
            Vector3 positionRequested = _boardSystem.GetHighlightPosition();
            if (TryGetPieceAtPosition(positionRequested,
                    out LevPieceController pieceController))
            {
                if (_pieceControllerSelected is null)
                {
                    SelectPiece(pieceController);
                }
                else if (!_pieceControllerSelected.hasMoved)
                {
                    if (_pieceControllerSelected == pieceController)
                    {
                        UnselectPiece();
                    }
                    else
                    {
                        SelectPiece(pieceController);
                    }
                }
            }
        }
    }

    public List<Vector3> PiecePositions()
    {
        List<Vector3> piecePositions = new(_pieceControllers.Count);

        foreach (LevPieceController pieceController in _pieceControllers)
        {
            piecePositions.Add(pieceController.piecePos);
        }

        return piecePositions;
    }

    public bool TryGetPieceAtPosition(Vector3 position, out LevPieceController playerController)
    {
        foreach (LevPieceController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.piecePos == position)
            {
                playerController = levPlayerController;
                return true;
            }
        }

        playerController = default;
        return false;
    }

    public void SetStateForAllPieces(LevPieceController.States state)
    {
        foreach (LevPieceController enemyController in _pieceControllers)
        {
            enemyController.SetState(state);
        }
    }

    private void SelectPiece(LevPieceController pieceController)
    {
        _pieceControllerSelected = pieceController;
        pieceController.SetState(LevPieceController.States.FindingMove);
        _chainUISystem.SetChain(_pieceControllerSelected.capturedPieces);
        _validMovesSystem.UpdateValidMoves(pieceController.GetAllValidMovesOfCurrentPiece(), pieceController.piecePos);
    }

    public void UnselectPiece()
    {
        _pieceControllerSelected?.SetState(LevPieceController.States.WaitingForTurn);
        _pieceControllerSelected = null;
        _chainUISystem.UnsetChain();
        _validMovesSystem.HideAllValidMoves();
    }

    public void PieceCaptured(LevPieceController pieceController)
    {
        foreach (Piece capturedMove in pieceController.capturedPieces)
        {
            _chainUISystem.ShowNewPiece(capturedMove);
        }
        
        _pieceControllers.Remove(pieceController);
        
        pieceController.SetState(LevPieceController.States.NotInUse);

        if (_pieceControllers.Count == 0)
        {
            _blackSystem.SetStateForAllPieces(LevPieceController.States.EndGame);
            SetStateForAllPieces(LevPieceController.States.EndGame);
            _endGameSystem.SetEndGame(LevPieceController.PieceColour.Black);
        }
    }
    
    public void SelectRandomPiece()
    {
        int enemySelectedIndex = Random.Range(0, _pieceControllers.Count);
        _pieceControllerSelected = _pieceControllers[enemySelectedIndex];
        _pieceControllerSelected.SetState(LevPieceController.States.FindingMove);
        _chainUISystem.SetChain(_pieceControllerSelected.capturedPieces);
        _validMovesSystem.UpdateSelectedBackground(_pieceControllerSelected.piecePos);
    }
}
