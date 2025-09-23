using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlackSystem : Dependency
{
    private ValidMovesSystem _validMovesSystem;
    private TurnSystem _turnSystem;
    private EndGameSystem _endGameSystem;
    private WhiteSystem _whiteSystem;
    
    private List<AIController> _pieceControllers = new ();
    private List<AIController> _piecesToMoveThisTurn = new();
    
    private bool _lost;
    
    private int _endTurnFrameCount;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
    }

    public override void Clean()
    {
        foreach (AIController aiController in _pieceControllers)
        {
            aiController.Destroy();
        }
        
        _pieceControllers.Clear();
    }

    public void CreatePiece(Vector2 position, Piece startingPiece, PieceAbility ability)
    {
        AIController pieceController = new AIController();
        pieceController.GameStart(Creator);
        pieceController.Init(position, startingPiece, ability);
        
        _pieceControllers.Add(pieceController);
    }

    public override void GameUpdate(float dt)
    {
        if (_endTurnFrameCount > 0)
        {
            _endTurnFrameCount--;
            if (_endTurnFrameCount == 0)
            {
                _turnSystem.SwitchTurn(PieceColour.White);
            }
            
            return;
        }

        for (int i = 0; i < _pieceControllers.Count; i++)
        {
            _pieceControllers[i].GameUpdate(dt);
        }
    }
    
    public bool TickAlwaysMovers()
    {
        bool found = false;
        foreach (AIController aiController in _pieceControllers)
        {
            if (aiController.pieceAbility == PieceAbility.AlwaysMove)
            {
                aiController.SetState(PieceState.FindingMove);
                found = true;
            }
        }

        return found;
    }

    public bool PieceCaptured(AIController capturedPieceController)
    {
        _pieceControllers.Remove(capturedPieceController);
        capturedPieceController.SetState(PieceState.NotInUse);
        capturedPieceController.Destroy();
        
        return _pieceControllers.Count == 0;
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

        foreach (AIController pieceController in _pieceControllers)
        {
            if (pieceController.pieceAbility == PieceAbility.MustMove
                || pieceController.pieceAbility == PieceAbility.AlwaysMove)
            {
                _piecesToMoveThisTurn.Add(pieceController);
            }
        }
        
        if (_piecesToMoveThisTurn.Count == 0)
        {
            Lose(GameOverReason.Captured, 1.1f);
        }
        else
        {
            foreach (AIController aiController in _piecesToMoveThisTurn)
            {
                if (aiController.pieceAbility != PieceAbility.AlwaysMove)
                {
                    aiController.PlayEnlargeAnimation();
                }
                
                aiController.SetState(PieceState.FindingMove);
            }
        }
    }
    
    private AIController SelectRandomPiece()
    {
        List<AIController> movablePieceControllers = MovablePieceControllers();
        
        if (movablePieceControllers.Count == 0)
        {
            return null;
        }
        
        int enemySelectedIndex = Random.Range(0, movablePieceControllers.Count);
        
        return movablePieceControllers[enemySelectedIndex];
    }

    private AIController FindPieceThatCanCapture()
    {
        List<AIController> movablePieceControllers = MovablePieceControllers();

        foreach (AIController pieceController in movablePieceControllers)
        {
            List<ValidMove> validMoves = pieceController.GetAllValidMovesOfPiece();
            
            foreach (ValidMove validMove in validMoves)
            {
                //Don't need to fear this inner for loop as it will basically always have 1 piece
                //At least for the puzzle levels

                if (_whiteSystem.playerController.piecePos == (Vector3)validMove.position)
                {
                    return pieceController;
                }
            }
        }

        return null;
    }
    
    private List<AIController> MovablePieceControllers()
    {
        List<AIController> validPieceControllers = new();
        
        foreach (AIController levPieceController in _pieceControllers)
        {
            if (levPieceController.GetAllValidMovesOfPiece().Count > 0 
                && levPieceController.pieceAbility != PieceAbility.MustMove //Must Moves will always move after the random piece has moved
                && levPieceController.pieceAbility != PieceAbility.AlwaysMove) //Always Moves either move on their own
            {
                validPieceControllers.Add(levPieceController);
            }
        }
        
        return validPieceControllers;
    }
    
    public void SpawnPieces(Level levelOnLoad)
    {
        foreach (StartingPieceSpawnData pieceSpawnData in levelOnLoad.positions)
        {
            if (pieceSpawnData.colour == PieceColour.Black)
            {
                CreatePiece(pieceSpawnData.position, pieceSpawnData.piece, pieceSpawnData.ability);
            }
        }
    }

    public List<Vector3> PiecePositions()
    {
        List<Vector3> piecePositions = new(_pieceControllers.Count);

        foreach (AIController pieceController in _pieceControllers)
        {
            piecePositions.Add(pieceController.piecePos);
            piecePositions.Add(pieceController.jumpPos);
        }

        return piecePositions;
    }

    public AIController GetPieceAtPosition(Vector3 position)
    {
        foreach (AIController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.piecePos == position)
            {
                return levPlayerController;
            }
        }
        
        return null;
    }

    public bool TryGetCaptureLoverMovingToPosition(Vector3 position, out AIController playerController)
    {
        foreach (AIController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.pieceAbility != PieceAbility.CaptureLover)
            {
                continue;
            }
            
            List<ValidMove> validMoves = levPlayerController.GetAllValidMovesOfPiece();
            foreach (ValidMove validMove in validMoves)
            {
                if ((Vector3)validMove.position == position)
                {
                    playerController = levPlayerController;
                    return true;
                }
            }
        }

        playerController = null;
        return false;
    }

    public void SetStateForAllPieces(PieceState state)
    {
        foreach (AIController enemyController in _pieceControllers)
        {
            enemyController.SetState(state);
        }
    }

    /// <summary>
    /// This just helps tick the enemy piece that's going to capture the player
    /// </summary>
    public void SelectCaptureLoverPiece(AIController pieceController, float3 movePosition)
    {
        pieceController.ForceMove(movePosition);
        pieceController.PlayEnlargeAnimation();
    }

    public bool IsPieceMoving()
    {
        foreach (AIController pieceController in _pieceControllers)
        {
            if (pieceController.state == PieceState.Moving)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPieceAtPosition(Vector3 position)
    {
        foreach (AIController pieceController in _pieceControllers)
        {
            float d1 = math.distance(pieceController.piecePos, position);
            float d2 = math.distance(pieceController.jumpPos, position);
            
            if (d1 < 0.01f || d2 < 0.01f)
            {
                return true;
            }
        }
        
        return false;
    }

    public void PieceBlocked(AIController pieceController)
    {
        _pieceControllers.Remove(pieceController);
        
        if (_pieceControllers.Count == 0)
        {
            Lose(GameOverReason.Locked, 0);
        }
        else if(pieceController.pieceAbility != PieceAbility.AlwaysMove)
        {
            PieceFinished(pieceController);
        }
        
        pieceController.Destroy();
    }
    
    public void Lose(GameOverReason gameOverReason, float delayTimer)
    {
        if (_lost)
        {
            return;
        }
        
        _whiteSystem.playerController.SetState(PieceState.EndGame);
        SetStateForAllPieces(PieceState.Blocked);
        _endGameSystem.SetEndGame(PieceColour.White, gameOverReason, delayTimer);
        _validMovesSystem.HideAllValidMoves();
        
        SetLost(true);
    }
    
    public void SetLost(bool lost)
    {
        _lost = lost;
    }

    public void PieceFinished(AIController pieceController)
    {
        _piecesToMoveThisTurn.Remove(pieceController);

        if (_piecesToMoveThisTurn.Count == 0)
        {
            _turnSystem.SwitchTurn(PieceColour.White);
        }
    }
}
