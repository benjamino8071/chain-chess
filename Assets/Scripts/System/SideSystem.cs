using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SideSystem : Dependency
{
    private AudioSystem _audioSystem;
    private ValidMovesSystem _validMovesSystem;
    private TurnSystem _turnSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    protected SideSystem _enemySideSystem;

    public PieceController pieceControllerSelected => _pieceControllerSelected;
    
    public ControlledBy controlledBy => _controlledBy;
    
    public bool frozen => _frozen;
    
    private List<PieceController> _pieceControllers = new ();
    private List<PieceController> _piecesToMoveThisTurn = new();
    
    private PieceController _pieceControllerSelected;
    
    protected PieceColour _allyPieceColour;
    protected PieceColour _enemyPieceColour;
    protected ControlledBy _controlledBy;
    protected bool _frozen;
    protected bool _tickCaptureLover;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        
        SpawnPieces();
    }

    public override void GameUpdate(float dt)
    {
        if (_turnSystem.CurrentTurn() == _enemyPieceColour && !_tickCaptureLover)
        {
            return;
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
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
        _tickCaptureLover = false;
    }

    public void SpawnPieces()
    {
        Level levelOnLoad = Creator.levelsSo.GetLevelOnLoad();
        foreach (PieceSpawnData pieceSpawnData in levelOnLoad.positions)
        {
            if (pieceSpawnData.colour == _allyPieceColour)
            {
                PieceController playerController = controlledBy == ControlledBy.Player 
                    ? new PlayerController() : new AIController();
                playerController.GameStart(Creator);
                playerController.Init(pieceSpawnData.position, pieceSpawnData.pieces, _allyPieceColour, pieceSpawnData.ability, 
                    controlledBy, this, _enemySideSystem);
            
                _pieceControllers.Add(playerController);
            }
        }
    }

    public List<Vector3> PiecePositions()
    {
        List<Vector3> piecePositions = new(_pieceControllers.Count);

        foreach (PieceController pieceController in _pieceControllers)
        {
            piecePositions.Add(pieceController.piecePos);
        }

        return piecePositions;
    }

    public bool TryGetAllyPieceAtPosition(Vector3 position, out PieceController playerController)
    {
        foreach (PieceController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.piecePos == position)
            {
                playerController = levPlayerController;
                return true;
            }
        }

        playerController = null;
        return false;
    }

    public bool TryGetCaptureLoverMovingToPosition(Vector3 position, out PieceController playerController)
    {
        foreach (PieceController levPlayerController in _pieceControllers)
        {
            if (levPlayerController.pieceAbility != PieceAbility.CaptureLover)
            {
                continue;
            }
            
            List<Vector3> validMoves = levPlayerController.AllValidMovesOfFirstPiece();
            if (validMoves.Contains(position))
            {
                playerController = levPlayerController;
                return true;
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
        pieceController.SetState(PieceController.States.FindingMove);
        _validMovesSystem.UpdateValidMoves(pieceController.GetAllValidMovesOfCurrentPiece());
    }

    /// <summary>
    /// This just helps tick the enemy piece that's going to capture the player
    /// </summary>
    public void SelectCaptureLoverPiece(PieceController pieceController)
    {
        _pieceControllerSelected = pieceController;
        pieceController.SetState(PieceController.States.FindingMove);
        _tickCaptureLover = true;
    }

    public void DeselectPiece()
    {
        if (_pieceControllerSelected is { movesRemaining: 0 })
        {
            _pieceControllerSelected?.SetState(PieceController.States.WaitingForTurn);
        }
        _pieceControllerSelected = null;
        _validMovesSystem.HideAllValidMoves();
    }

    public void FreezeSide()
    {
        DeselectPiece();
        _frozen = true;
    }
    
    /// <returns>True = all ally pieces captured</returns>
    public bool PieceCaptured(PieceController capturedPieceController, PieceController pieceUsed)
    {
        _pieceControllers.Remove(capturedPieceController);
        
        capturedPieceController.SetState(PieceController.States.NotInUse);
        
        if (_pieceControllers.Count == 0)
        {
            Lose(GameOverReason.Captured);
            _chainUISystem.HighlightNextPiece(pieceUsed);
            return true;
        }
        else
        {
            float pitchAmount = 1 + 0.02f * pieceUsed.piecesCapturedInThisTurn;
            
            _audioSystem.PlayPieceCapturedSfx(pitchAmount);
            return false;
        }
    }

    public void PieceLocked(PieceController pieceController)
    {
        _pieceControllers.Remove(pieceController);
        
        pieceController.SetState(PieceController.States.NotInUse);
        
        if (_pieceControllers.Count == 0)
        {
            Lose(GameOverReason.Locked);
        }
    }

    public void Lose(GameOverReason gameOverReason)
    {
        _enemySideSystem.SetStateForAllPieces(PieceController.States.EndGame);
        SetStateForAllPieces(PieceController.States.EndGame);
        _endGameSystem.SetEndGame(_enemyPieceColour, gameOverReason);
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
            
            _pieceControllerSelected.SetState(PieceController.States.FindingMove);
        }
    }

    public void AiSetup()
    {
        _piecesToMoveThisTurn = new(_pieceControllers.Count);
        
        if (SelectRandomPiece() is { } randomPiece)
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
            Lose(GameOverReason.Captured);
        }
        else
        {
            _pieceControllerSelected = _piecesToMoveThisTurn[0];

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

    private List<PieceController> MovablePieceControllers()
    {
        List<PieceController> validPieceControllers = new();

        foreach (PieceController levPieceController in _pieceControllers)
        {
            if (levPieceController.AllValidMovesOfFirstPiece().Count > 0 
                && levPieceController.pieceAbility != PieceAbility.MustMove) //Must Moves will always move after the random piece has moved
            {
                validPieceControllers.Add(levPieceController);
            }
        }
        
        return validPieceControllers;
    }
}
