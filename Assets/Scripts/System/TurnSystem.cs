using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private SettingsUISystem _settingsUISystem;
    private ValidMovesSystem _validMovesSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    private BoardSystem _boardSystem;
    private LevelSelectUISystem _levelSelectUISystem;
    private PlayerSetTilesSystem _playerSetTilesSystem;
    
    private PieceColour _currentTurn; //Will always start with White
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _levelSelectUISystem = creator.GetDependency<LevelSelectUISystem>();
        _playerSetTilesSystem = creator.GetDependency<PlayerSetTilesSystem>();
        
        LoadLevelRuntime();
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
                Creator.statsTurns++;
                
                _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfFirstPiece());
                
                _whiteSystem.playerController.SetState(PieceState.FindingMove);
                _blackSystem.SetStateForAllPieces(PieceState.WaitingForTurn);
                
                _chainUISystem.ShowChain(_whiteSystem.playerController, false);
                
                _settingsUISystem.ShowButton();
                break;
            case PieceColour.Black:
                _settingsUISystem.HideButton();
                
                _validMovesSystem.HideAllValidMoves();
                
                _whiteSystem.playerController.SetState(PieceState.WaitingForTurn);
                _blackSystem.AiSetup();
                break;
        }
    }
    
    public void LoadLevelRuntime()
    {
        Random.InitState(42);
       
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _playerSetTilesSystem.HideAll();
        
        _levelSelectUISystem.UpdateLevelText(Creator.levelsSo.levelOnLoad);
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        _chainUISystem.ShowChain(_whiteSystem.playerController, true);
        _boardSystem.ShowTapPoint(_whiteSystem.playerController.piecePos);
        _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfCurrentPiece());
        
        _endGameSystem.ResetEndGame();

        Creator.statsTurns = 0;
        Creator.statsMoves = 0;
        
        SwitchTurn(PieceColour.White);
    }

    public PieceColour CurrentTurn()
    {
        return _currentTurn;
    }
}
