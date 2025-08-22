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
    private TurnSystem _turnSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    protected SideSystem _enemySideSystem;
    private SettingsUISystem _settingsUISystem;

    public PieceController pieceControllerSelected => _pieceControllerSelected;
    
    public ControlledBy controlledBy => _controlledBy;
    
    public bool frozen => _frozen;
    
    private List<PieceController> _pieceControllers = new ();
    private List<PieceController> _piecesToMoveThisTurn = new();
    private List<PieceController> _mustMovers = new();
    
    private PieceController _pieceControllerSelected;
    
    protected PieceColour _allyPieceColour;
    protected PieceColour _enemyPieceColour;
    protected ControlledBy _controlledBy;
    protected bool _frozen;
    protected bool _tickCaptureLover;
    protected int _endTurnFrameCount;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        
        SpawnPieces();
    }

    public override void GameUpdate(float dt)
    {
        if (_endTurnFrameCount > 0)
        {
            _endTurnFrameCount--;
            if (_endTurnFrameCount == 0)
            {
                _turnSystem.SwitchTurn(_enemyPieceColour);
            }
        }
        
        foreach (PieceController pieceController in _mustMovers)
        {
            pieceController.GameUpdate(dt);
        }
        
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
        _mustMovers.Clear();
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
                CreatePiece(pieceSpawnData.position, pieceSpawnData.pieces, pieceSpawnData.ability);
            }
        }
    }

    public void CreatePiece(Vector2 position, List<Piece> pieceMoves, PieceAbility ability)
    {
        PieceController pieceController = controlledBy == ControlledBy.Player 
            ? new PlayerController() : new AIController();
        pieceController.GameStart(Creator);
        pieceController.Init(position, pieceMoves, _allyPieceColour, ability, 
            controlledBy, this, _enemySideSystem);
        
        _pieceControllers.Add(pieceController);
        
        if (ability == PieceAbility.AlwaysMove)
        {
            _mustMovers.Add(pieceController);
            pieceController.SetState(PieceController.States.FindingMove);
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
        
        List<Vector3> validMoves = pieceController.GetAllValidMovesOfCurrentPiece();
        if (validMoves.Count == 0)
        {
            Debug.Log("THIS PIECE CAN'T MOVE");
            //PieceBlocked(pieceController);
            pieceController.SetState(PieceController.States.Blocked);
            return;
        }
        
        pieceController.SetState(PieceController.States.FindingMove);
        Debug.Log("Piece selected. Update valid moves!");
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
        _tickCaptureLover = true;
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
        _validMovesSystem.HideAllValidMoves();
    }

    public void ForceDeselectPiece()
    {
        _pieceControllerSelected = null;
    }

    public void FreezeSide()
    {
        _pieceControllerSelected?.SetState(PieceController.States.WaitingForTurn);
        _pieceControllerSelected = null;
        _validMovesSystem.HideAllValidMoves();
        
        //DeselectPiece();
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

    public void PieceBlocked(PieceController pieceController)
    {
        _pieceControllers.Remove(pieceController);
        
        //pieceController.SetState(PieceController.States.Blocked);
        
        if (_pieceControllers.Count == 0)
        {
            Lose(GameOverReason.Locked);
        }
        else
        {
            PieceFinished(pieceController);
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
            
            _pieceControllerSelected.PlayEnlargeAnimation();
            _pieceControllerSelected.SetState(PieceController.States.FindingMove);
        }
    }

    public void AiSetup()
    {
        _piecesToMoveThisTurn = new(_pieceControllers.Count);
        
        /*
         * Try to find a piece that can capture the other player
         */
        SystemRandom rnd = new(DateTime.Now.Millisecond);
        bool findPieceThatCanCapture = rnd.Next(1, 100) <= Creator.piecesSo.capturePlayerOdds;
        
        if(findPieceThatCanCapture && FindPieceThatCanCapture() is {} pieceThatCanCapture)
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
            if (_mustMovers.Count > 0)
            {
                _endTurnFrameCount = 2;
            }
            else
            {
                Lose(GameOverReason.Captured);
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
        List<PieceController> enemyPieceControllers = _enemySideSystem.MovablePieceControllers();
        
        List<PieceController> movablePieceControllers = MovablePieceControllers();

        foreach (PieceController pieceController in movablePieceControllers)
        {
            List<Vector3> validMoves = pieceController.GetAllValidMovesOfCurrentPiece();

            foreach (Vector3 validMove in validMoves)
            {
                //Don't need to fear this inner for loop as it will basically always have 1 piece
                //At least for the puzzle levels
                foreach (PieceController enemyPieceController in enemyPieceControllers)
                {
                    if (enemyPieceController.piecePos == validMove)
                    {
                        return pieceController;
                    }
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
                && levPieceController.pieceAbility != PieceAbility.AlwaysMove) //Always Moves just move on their own
            {
                validPieceControllers.Add(levPieceController);
            }
        }
        
        return validPieceControllers;
    }
}
