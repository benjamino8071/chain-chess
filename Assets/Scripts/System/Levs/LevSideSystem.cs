using System.Collections.Generic;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevSideSystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevBoardSystem _boardSystem;
    private LevTurnSystem _turnSystem;
    private LevChainUISystem _chainUISystem;
    private LevEndGameSystem _endGameSystem;
    private LevPauseUISystem _pauseUISystem;
    protected LevSideSystem _enemySideSystem;

    public LevPieceController pieceControllerSelected => _pieceControllerSelected;
    public List<LevPieceController> pieceControllers => _pieceControllers;
    
    public ControlledBy controlledBy => _controlledBy;
    
    private List<LevPieceController> _pieceControllers = new ();

    private LevPieceController _pieceControllerSelected;
    
    protected PieceColour _allyPieceColour;
    protected PieceColour _enemyPieceColour;
    protected ControlledBy _controlledBy;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();
        _endGameSystem = levCreator.GetDependency<LevEndGameSystem>();
        
        SpawnPieces();
    }

    public override void GameUpdate(float dt)
    {
        if (_turnSystem.CurrentTurn() == _enemyPieceColour)
        {
            return;
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
    }

    public override void Clean()
    {
        _pieceControllerSelected = null;
        foreach (LevPieceController levPieceController in _pieceControllers)
        {
            levPieceController.Destroy();
        }
        _pieceControllers.Clear();
    }

    public void SpawnPieces()
    {
        Level levelOnLoad = Creator.levelsSo.GetLevelOnLoad();
        foreach (PieceSpawnData pieceSpawnData in levelOnLoad.positions)
        {
            if (pieceSpawnData.colour == _allyPieceColour)
            {
                LevPieceController levPlayerController = controlledBy == ControlledBy.Player 
                    ? new LevPlayerController() : new LevAIController();
                levPlayerController.GameStart(Creator);
                Vector3 actualPos = pieceSpawnData.position + new Vector2(0.5f, 0.5f);
                levPlayerController.Init(actualPos, pieceSpawnData.piece, _allyPieceColour, controlledBy);
            
                _pieceControllers.Add(levPlayerController);
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

    public bool TryGetAllyPieceAtPosition(Vector3 position, out LevPieceController playerController)
    {
        foreach (LevPieceController levPlayerController in _pieceControllers)
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

    public void SetStateForAllPieces(LevPieceController.States state)
    {
        foreach (LevPieceController enemyController in _pieceControllers)
        {
            enemyController.SetState(state);
        }
    }

    public void SelectPiece(LevPieceController pieceController)
    {
        _pieceControllerSelected = pieceController;
        pieceController.SetState(LevPieceController.States.FindingMove);
        _validMovesSystem.UpdateValidMoves(pieceController.GetAllValidMovesOfCurrentPiece());
    }

    public void DeselectPiece()
    {
        if (_pieceControllerSelected is { movesRemaining: 0 })
        {
            _pieceControllerSelected?.SetState(LevPieceController.States.WaitingForTurn);
        }
        _pieceControllerSelected = null;
        _validMovesSystem.HideAllValidMoves();
    }
    
    /// <returns>True = all ally pieces captured</returns>
    public bool PieceCaptured(LevPieceController capturedPieceController, int takerPiecesCapturedThisTurn, int takerMovesUsed)
    {
        _pieceControllers.Remove(capturedPieceController);
        
        capturedPieceController.SetState(LevPieceController.States.NotInUse);
        
        if (_pieceControllers.Count == 0)
        {
            Lose();
            _chainUISystem.HighlightNextPiece(takerMovesUsed);
            return true;
        }
        else
        {
            float pitchAmount = 1 + 0.02f * takerPiecesCapturedThisTurn;
            
            _audioSystem.PlayPieceCapturedSfx(pitchAmount);
            return false;
        }
    }

    public void PieceLocked(LevPieceController pieceController)
    {
        _pieceControllers.Remove(pieceController);
        
        pieceController.SetState(LevPieceController.States.NotInUse);
        
        if (_pieceControllers.Count == 0)
        {
            Lose();
        }
    }

    public void Lose()
    {
        _enemySideSystem.SetStateForAllPieces(LevPieceController.States.EndGame);
        SetStateForAllPieces(LevPieceController.States.EndGame);
        _endGameSystem.SetEndGame(_enemyPieceColour);
        _validMovesSystem.HideAllValidMoves();
    }
    
    public void SelectRandomPiece()
    {
        List<LevPieceController> movablePieceControllers = MovablePieceControllers();
        if (movablePieceControllers.Count == 0)
        {
            Lose();
            return;
        }
        
        int enemySelectedIndex = Random.Range(0, movablePieceControllers.Count);
        _pieceControllerSelected = movablePieceControllers[enemySelectedIndex];
        
        _pieceControllerSelected.SetState(LevPieceController.States.FindingMove);
    }

    private List<LevPieceController> MovablePieceControllers()
    {
        List<LevPieceController> validPieceControllers = new();

        foreach (LevPieceController levPieceController in _pieceControllers)
        {
            if (levPieceController.AllValidMovesOfFirstPiece().Count > 0)
            {
                validPieceControllers.Add(levPieceController);
            }
        }
        
        return validPieceControllers;
    }
}
