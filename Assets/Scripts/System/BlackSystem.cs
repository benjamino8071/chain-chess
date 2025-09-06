using System.Collections.Generic;
using UnityEngine;

public class BlackSystem : SideSystem
{
    private int _endTurnFrameCount;
    
    private List<PieceController> _alwaysMovers = new();
    
    public override void GameStart(Creator creator)
    {
        _allyPieceColour = PieceColour.Black;
        _enemyPieceColour = PieceColour.White;
        _enemySideSystem = creator.GetDependency<WhiteSystem>();

        base.GameStart(creator);
    }

    public override void Clean()
    {
        base.Clean();
        _alwaysMovers.Clear();
    }

    public override void CreatePiece(Vector2 position, Piece startingPiece, PieceAbility ability)
    {
        PieceController pieceController = new AIController();
        pieceController.GameStart(Creator);
        pieceController.Init(position, startingPiece, _allyPieceColour, ability, this, _enemySideSystem);
        
        if (ability == PieceAbility.AlwaysMove)
        {
            _alwaysMovers.Add(pieceController);
        }
        
        _pieceControllers.Add(pieceController);
    }

    public override void GameUpdate(float dt)
    {
        if (_endTurnFrameCount > 0)
        {
            _endTurnFrameCount--;
            if (_endTurnFrameCount == 0)
            {
                _turnSystem.SwitchTurn(_enemyPieceColour);
                return;
            }
        }
        
        foreach (PieceController pieceController in _alwaysMovers)
        {
            pieceController.GameUpdate(dt);
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
    }
    
    public bool TickAlwaysMovers()
    {
        foreach (PieceController pieceController in _alwaysMovers)
        {
            pieceController.SetState(PieceController.States.FindingMove);
        }

        return _alwaysMovers.Count > 0;
    }

    public override bool PieceCaptured(PieceController capturedPieceController)
    {
        _alwaysMovers.Remove(capturedPieceController);
        
        return base.PieceCaptured(capturedPieceController);
    }

    public void AiSetup()
    {
        _piecesToMoveThisTurn = new(_pieceControllers.Count);
        
        /*
         * Try to find a piece that can capture the other player
         */
        if(FindPieceThatCanCapture() is {} pieceThatCanCapture)
        {
            _piecesToMoveThisTurn.Add(pieceThatCanCapture);
        }
        else if (SelectRandomPiece() is { } randomPiece)
        {
            _piecesToMoveThisTurn.Add(randomPiece);
        }

        foreach (PieceController pieceController in _pieceControllers)
        {
            if (pieceController.pieceAbility == PieceAbility.MustMove)
            {
                _piecesToMoveThisTurn.Add(pieceController);
            }
        }
        
        if (_piecesToMoveThisTurn.Count == 0)
        {
            if (_alwaysMovers.Count > 0)
            {
                _endTurnFrameCount = 2;
            }
            else
            {
                Lose(GameOverReason.Captured, 1.1f);
            }
        }
        else
        {
            _pieceControllerSelected = _piecesToMoveThisTurn[0];

            _pieceControllerSelected.PlayEnlargeAnimation();
            _pieceControllerSelected.SetState(PieceController.States.FindingMove);
        }
    }
    
    private PieceController SelectRandomPiece()
    {
        List<PieceController> movablePieceControllers = MovablePieceControllers();
        if (movablePieceControllers.Count == 0)
        {
            return null;
        }
        
        int enemySelectedIndex = Random.Range(0, movablePieceControllers.Count);
        
        return movablePieceControllers[enemySelectedIndex];
    }

    private PieceController FindPieceThatCanCapture()
    {
        PieceController whitePieceController = _enemySideSystem.pieceControllerSelected;
        
        List<PieceController> movablePieceControllers = MovablePieceControllers();

        foreach (PieceController pieceController in movablePieceControllers)
        {
            List<ValidMove> validMoves = pieceController.GetAllValidMovesOfCurrentPiece();

            foreach (ValidMove validMove in validMoves)
            {
                //Don't need to fear this inner for loop as it will basically always have 1 piece
                //At least for the puzzle levels

                if (whitePieceController.piecePos == (Vector3)validMove.position)
                {
                    return pieceController;
                }
            }
        }

        return null;
    }
    
    private List<PieceController> MovablePieceControllers()
    {
        List<PieceController> validPieceControllers = new();
        
        foreach (PieceController levPieceController in _pieceControllers)
        {
            if (levPieceController.AllValidMovesOfFirstPiece().Count > 0 
                && levPieceController.pieceAbility != PieceAbility.MustMove //Must Moves will always move after the random piece has moved
                && levPieceController.pieceAbility != PieceAbility.AlwaysMove) //Always Moves either move on their own
            {
                validPieceControllers.Add(levPieceController);
            }
        }
        
        return validPieceControllers;
    }
}
