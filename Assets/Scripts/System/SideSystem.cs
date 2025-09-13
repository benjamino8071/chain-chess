using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using SystemRandom = System.Random;

public class SideSystem : Dependency
{
    private AudioSystem _audioSystem;
    private ValidMovesSystem _validMovesSystem;
    protected TurnSystem _turnSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    protected SideSystem _enemySideSystem;
    private SettingsUISystem _settingsUISystem;

    public PieceController pieceControllerSelected => _pieceControllerSelected;
    
    protected List<PieceController> _pieceControllers = new ();
    protected List<PieceController> _piecesToMoveThisTurn = new();
    
    protected PieceController _pieceControllerSelected;
    
    protected PieceColour _allyPieceColour;
    protected PieceColour _enemyPieceColour;
    protected bool _frozen;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
    }

    public override void Clean()
    {
        _pieceControllerSelected = null;
        foreach (PieceController levPieceController in _pieceControllers)
        {
            levPieceController.Destroy();
        }
        _pieceControllers.Clear();
        _frozen = false;
    }

    public virtual void SpawnPieces()
    {
        Level levelOnLoad = Creator.levelsSo.GetLevelOnLoad();
        foreach (StartingPieceSpawnData pieceSpawnData in levelOnLoad.positions)
        {
            if (pieceSpawnData.colour == _allyPieceColour)
            {
                CreatePiece(pieceSpawnData.position, pieceSpawnData.piece, pieceSpawnData.ability);
            }
        }
    }

    public virtual void CreatePiece(Vector2 position, Piece startingPiece, PieceAbility ability)
    {
        
    }

    public List<Vector3> PiecePositions()
    {
        List<Vector3> piecePositions = new(_pieceControllers.Count);

        foreach (PieceController pieceController in _pieceControllers)
        {
            piecePositions.Add(pieceController.piecePos);
            piecePositions.Add(pieceController.jumpPos);
        }

        return piecePositions;
    }

    public PieceController GetPieceAtPosition(Vector3 position)
    {
        foreach (PieceController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.piecePos == position)
            {
                return levPlayerController;
            }
        }
        
        return null;
    }

    public bool TryGetCaptureLoverMovingToPosition(Vector3 position, out PieceController playerController)
    {
        foreach (PieceController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.pieceAbility != PieceAbility.CaptureLover)
            {
                continue;
            }
            
            List<ValidMove> validMoves = levPlayerController.AllValidMovesOfFirstPiece();
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

    public void SetStateForAllPieces(PieceController.States state)
    {
        foreach (PieceController enemyController in _pieceControllers)
        {
            enemyController.SetState(state);
        }
    }

    public void SelectPiece(PieceController pieceController)
    {
        _pieceControllerSelected = pieceController;
        
        List<ValidMove> validMoves = pieceController.GetAllValidMovesOfCurrentPiece();
        if (validMoves.Count == 0)
        {
            Debug.Log("THIS PIECE CAN'T MOVE");
            //PieceBlocked(pieceController);
            pieceController.SetState(PieceController.States.Blocked);
            return;
        }
        
        pieceController.SetState(PieceController.States.FindingMove);
        _validMovesSystem.UpdateValidMoves(pieceController.GetAllValidMovesOfCurrentPiece());
    }

    public void UpdateSelectedPieceValidMoves()
    {
        if (_pieceControllerSelected is {} pieceController && pieceController.state != PieceController.States.Moving)
        {
            _validMovesSystem.UpdateValidMoves(_pieceControllerSelected.GetAllValidMovesOfCurrentPiece());
        }
    }

    /// <summary>
    /// This just helps tick the enemy piece that's going to capture the player
    /// </summary>
    public void SelectCaptureLoverPiece(PieceController pieceController, float3 movePosition)
    {
        _pieceControllerSelected = pieceController;
        pieceController.ForceMove(movePosition);
        pieceController.PlayEnlargeAnimation();
    }

    public void DeselectPiece()
    {
        if (_pieceControllerSelected is { } pieceController)
        {
            if (pieceController.state is PieceController.States.Blocked or PieceController.States.Moving)
            {
                return;
            }
            if (pieceController.movesRemaining == 0)
            {
                _pieceControllerSelected?.SetState(PieceController.States.WaitingForTurn);
            }
        }
        
        _pieceControllerSelected = null;
    }

    public void ForceDeselectPiece()
    {
        _pieceControllerSelected = null;
    }

    public void FreezeSide()
    {
        _pieceControllerSelected?.SetState(PieceController.States.WaitingForTurn);
        _validMovesSystem.HideAllValidMoves();
        
        _frozen = true;
    }

    public void UnfreezeSide()
    {
        _frozen = false;
    }

    public bool IsPieceMoving()
    {
        foreach (PieceController pieceController in _pieceControllers)
        {
            if (pieceController.state == PieceController.States.Moving)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <returns>True = all ally pieces captured</returns>
    public virtual bool PieceCaptured(PieceController capturedPieceController)
    {
        _pieceControllers.Remove(capturedPieceController);
        capturedPieceController.SetState(PieceController.States.NotInUse);
        capturedPieceController.Destroy();
        
        return _pieceControllers.Count == 0;
    }

    public virtual void PieceBlocked(PieceController pieceController)
    {
        _pieceControllers.Remove(pieceController);
        
        if (_pieceControllers.Count == 0)
        {
            Lose(GameOverReason.Locked, 0);
        }
        else
        {
            PieceFinished(pieceController);
        }
        
        pieceController.Destroy();
    }

    public void Lose(GameOverReason gameOverReason, float delayTimer)
    {
        _enemySideSystem.SetStateForAllPieces(PieceController.States.EndGame);
        SetStateForAllPieces(PieceController.States.Blocked);
        _endGameSystem.SetEndGame(_enemyPieceColour, gameOverReason, delayTimer);
        _validMovesSystem.HideAllValidMoves();
    }

    public void PieceFinished(PieceController pieceController)
    {
        _piecesToMoveThisTurn.Remove(pieceController);

        if (_piecesToMoveThisTurn.Count == 0)
        {
            _turnSystem.SwitchTurn(_enemyPieceColour);
        }
        else
        {
            _pieceControllerSelected = _piecesToMoveThisTurn[0];
            
            _pieceControllerSelected.PlayEnlargeAnimation();
            _pieceControllerSelected.SetState(PieceController.States.FindingMove);
        }
    }
}
