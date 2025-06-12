using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LevSideSystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevBoardSystem _boardSystem;
    private LevTurnSystem _turnSystem;
    private LevChainUISystem _chainUISystem;
    private LevEndGameSystem _endGameSystem;
    protected LevSideSystem _enemySideSystem;
    
    private List<LevPieceController> _pieceControllers = new ();

    private LevPieceController _pieceControllerSelected;
    
    protected PieceColour allyPieceColour;
    protected PieceColour enemyPieceColour;
    protected ControlledBy controlledBy;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _endGameSystem = levCreator.GetDependency<LevEndGameSystem>();
        
        SpawnPieces();
    }

    public override void GameUpdate(float dt)
    {
        if (_turnSystem.CurrentTurn() == enemyPieceColour)
        {
            return;
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
        
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame() && controlledBy == ControlledBy.Player && _boardSystem.GetMouseWorldPosition().y < 0.8f)
        {
            /*
             * If the player selects another piece when they haven't made a move
             * then we want to select that piece
             */
            
            Vector3 positionRequested = _boardSystem.GetHighlightPosition();
            if (TryGetAllyPieceAtPosition(positionRequested,
                    out LevPieceController pieceController))
            {
                if (_pieceControllerSelected is null)
                {
                    SelectPiece(pieceController);
                    Creator.playerSystemSo.hideMainMenuTrigger = true;
                    _audioSystem.PlayPieceSelectedSfx(1);
                }
                else if (!_pieceControllerSelected.hasMoved)
                {
                    if (_pieceControllerSelected == pieceController)
                    {
                        UnselectPiece();
                        _audioSystem.PlayPieceSelectedSfx(0.8f);
                    }
                    else
                    {
                        SelectPiece(pieceController);
                        _audioSystem.PlayPieceSelectedSfx(1);
                    }
                }
            }
            else if (_pieceControllerSelected is { state: LevPieceController.States.FindingMove, hasMoved: false})
            {
                UnselectPiece();
                _audioSystem.PlayPieceSelectedSfx(0.8f);
            }
        }
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
            if (pieceSpawnData.colour == allyPieceColour)
            {
                LevPieceController levPlayerController = controlledBy == ControlledBy.Player 
                    ? new LevPlayerController() : new LevAIController();
                levPlayerController.GameStart(Creator);
                levPlayerController.Init(pieceSpawnData.position, pieceSpawnData.piece, allyPieceColour);
            
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
    
    /// <returns>True = all ally pieces captured</returns>
    public bool PieceCaptured(LevPieceController capturedPieceController, int takerPiecesCapturedThisTurn)
    {
        _pieceControllers.Remove(capturedPieceController);
        
        capturedPieceController.SetState(LevPieceController.States.NotInUse);
        
        if (_pieceControllers.Count == 0)
        {
            _enemySideSystem.SetStateForAllPieces(LevPieceController.States.EndGame);
            SetStateForAllPieces(LevPieceController.States.EndGame);
            _endGameSystem.SetEndGame(enemyPieceColour);
            _validMovesSystem.HideAllValidMoves();
            return true;
        }
        else
        {
            float pitchAmount = 1 + 0.02f * takerPiecesCapturedThisTurn;
            
            _audioSystem.PlayPieceCapturedSfx(pitchAmount);
            return false;
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
