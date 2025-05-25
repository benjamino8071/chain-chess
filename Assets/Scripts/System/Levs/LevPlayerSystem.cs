using System.Collections.Generic;
using UnityEngine;

public class LevPlayerSystem : LevDependency
{
    private LevGridSystem _gridSystem;
    private LevTurnSystem _turnSystem;
    private LevChainUISystem _chainUISystem;
    private LevGameOverUISystem _gameOverUISystem;
    
    private List<LevPlayerController> _playerControllers = new ();

    private LevPlayerController _playerControllerSelected;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _gridSystem = levCreator.GetDependency<LevGridSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();

        foreach (Transform playerSpawnPos in levCreator.GetObjectsByName(AllTagNames.PlayerSpawnPosition))
        {
            PieceType pieceType = playerSpawnPos.GetComponent<PieceType>();
            
            LevPlayerController levPlayerController = new LevPlayerController();
            levPlayerController.GameStart(levCreator);
            levPlayerController.SetPiece(playerSpawnPos, pieceType.piece);
            
            _playerControllers.Add(levPlayerController);
        }
    }

    public override void GameUpdate(float dt)
    {
        if(_turnSystem.CurrentTurn() == LevTurnSystem.Turn.Enemy)
            return;
        
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame())
        {
            /*
             * If the player selects another piece when they haven't made a move
             * then we want to select that piece
             */
            
            Vector3 positionRequested = _gridSystem.GetHighlightPosition();
            if (TryGetPlayerAtPosition(positionRequested,
                    out LevPlayerController playerController))
            {
                if (_playerControllerSelected is null)
                {
                    _playerControllerSelected = playerController;
                    _chainUISystem.SetChain(_playerControllerSelected.capturedPieces);
                }
                else if (!_playerControllerSelected.hasMoved)
                {
                    if (_playerControllerSelected == playerController)
                    {
                        UnselectPiece();
                    }
                    else
                    {
                        _playerControllerSelected = playerController;
                        _chainUISystem.SetChain(_playerControllerSelected.capturedPieces);
                    }
                }
            }
            else if (_playerControllerSelected is not null)
            {
                if (!_playerControllerSelected.TryMovePlayer(positionRequested) && !_playerControllerSelected.hasMoved)
                {
                    UnselectPiece();
                }
            }
        }
        
        foreach (LevPlayerController playerController in _playerControllers)
        {
            playerController.HideAllValidMoves();
            if (playerController == _playerControllerSelected)
            {
                playerController.UpdateValidMoves();
            }
            playerController.GameUpdate(dt);
        }
    }

    public bool IsPlayerAtPosition(Vector3 position)
    {
        foreach (LevPlayerController playerController in _playerControllers)
        {
            if (playerController.GetPlayerPosition() == position)
                return true;
        }

        return false;
    }

    public bool TryGetPlayerAtPosition(Vector3 position, out LevPlayerController playerController)
    {
        foreach (LevPlayerController levPlayerController in _playerControllers)
        {
            if (levPlayerController.GetPlayerPosition() == position)
            {
                playerController = levPlayerController;
                return true;
            }
        }

        playerController = default;
        return false;
    }

    public bool TrySetStateOfPlayerAtPosition(Vector3 position, LevPlayerController.States state)
    {
        foreach (LevPlayerController playerController in _playerControllers)
        {
            if (playerController.GetPlayerPosition() == position)
            {
                playerController.SetState(state);
                return true;
            }
        }

        return false;
    }

    public void SetStateForAllPlayers(LevPlayerController.States state)
    {
        foreach (LevPlayerController enemyController in _playerControllers)
        {
            if (enemyController.GetState() != LevPlayerController.States.Captured)
            {
                enemyController.SetState(state);
            }
        }
    }

    public bool AreAllPlayersCaptured()
    {
        foreach (LevPlayerController playerController in _playerControllers)
        {
            if (playerController.GetState() == LevPlayerController.States.Captured)
                return false;
        }

        return true;
    }

    public void UnselectPiece()
    {
        _playerControllerSelected = null;
        _chainUISystem.UnsetChain();
    }

    public void PieceCaptured(LevPlayerController playerController)
    {
        _playerControllers.Remove(playerController);
        
        playerController.SetState(LevPlayerController.States.Captured);

        if (_playerControllers.Count == 0)
        {
            _gameOverUISystem.Show();
        }
    }
}
