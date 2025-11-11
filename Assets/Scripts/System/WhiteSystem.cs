using UnityEngine;

public class WhiteSystem : Dependency
{
    private ValidMovesSystem _validMovesSystem;
    private TurnSystem _turnSystem;
    private EndGameSystem _endGameSystem;
    private BlackSystem _blackSystem;
    
    public PlayerController playerController => _playerController;
    
    private PlayerController _playerController;
    
    private bool _lost;
    
    private bool _frozen;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _blackSystem = creator.GetDependency<BlackSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
    }

    private void CreatePiece(Vector2 position, Piece startingPiece)
    {
        _playerController = new PlayerController();
        _playerController.GameStart(Creator);
        _playerController.Init(position, startingPiece);
    }

    public override void GameUpdate(float dt)
    {
        if (_frozen || _turnSystem.CurrentTurn() == PieceColour.Black)
        {
            return;
        }
        
        _playerController?.GameUpdate(dt);
    }
    
    public override void Clean()
    {
        _playerController?.Destroy();
        _playerController = null;
        
        _frozen = false;
    }

    public void SpawnPieces(Level levelOnLoad)
    {
        foreach (StartingPieceSpawnData pieceSpawnData in levelOnLoad.positions)
        {
            if (pieceSpawnData.colour == PieceColour.White)
            {
                CreatePiece(pieceSpawnData.position, pieceSpawnData.piece);
                break;
            }
        }
    }

    public void FreezeSide()
    {
        _playerController.SetState(PieceState.WaitingForTurn);
        _validMovesSystem.HideAllValidMoves();
        
        _frozen = true;
    }

    public void UnfreezeSide()
    {
        _frozen = false;
    }

    public void Lose(GameOverReason gameOverReason)
    {
        if (_lost)
        {
            return;
        }
        
        _blackSystem.SetStateForAllPieces(PieceState.EndGame);
        _playerController.SetState(PieceState.EndGame);
        _playerController.Destroy();
        _playerController = null;
        
        _endGameSystem.SetEndGame(PieceColour.Black, gameOverReason);
        _validMovesSystem.HideAllValidMoves();
        
        SetLost(true);
    }
    
    public void SetLost(bool lost)
    {
        _lost = lost;
    }
}
