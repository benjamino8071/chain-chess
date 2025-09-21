using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private UISystem _uiSystem;
    private UIChain _uiChain;
    private UISettings _uiSettings;
    private UICurrentLevelMenu _uiCurrentLevelMenu;
    private ValidMovesSystem _validMovesSystem;
    private EndGameSystem _endGameSystem;
    private BoardSystem _boardSystem;
    private PlayerSetTilesSystem _playerSetTilesSystem;

    public Level currentLevel => _currentLevel;
    
    private PieceColour _currentTurn; //Will always start with White

    private Level _currentLevel;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
        _uiChain = _uiSystem.GetUI<UIChain>();
        _uiSettings = _uiSystem.GetUI<UISettings>();
        _uiCurrentLevelMenu = _uiSystem.GetUI<UICurrentLevelMenu>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _playerSetTilesSystem = creator.GetDependency<PlayerSetTilesSystem>();
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
                
                _uiChain.ShowChain(_whiteSystem.playerController, false);
                break;
            case PieceColour.Black:
                _validMovesSystem.HideAllValidMoves();
                
                _whiteSystem.playerController.SetState(PieceState.WaitingForTurn);
                _blackSystem.AiSetup();
                break;
        }
    }

    public void ReloadCurrentLevel()
    {
        LoadLevelRuntime(_currentLevel);
    }
    
    public void LoadLevelRuntime(Level level)
    {
        Random.InitState(42);
       
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _playerSetTilesSystem.HideAll();
        
        _uiCurrentLevelMenu.SetLevelsButtonText(level);
        
        _whiteSystem.SpawnPieces(level);
        _blackSystem.SpawnPieces(level);
        _uiChain.ShowChain(_whiteSystem.playerController, true);
        _boardSystem.ShowTapPoint(_whiteSystem.playerController.piecePos);
        _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfCurrentPiece());
        
        _endGameSystem.ResetEndGame();

        Creator.statsTurns = 0;
        Creator.statsMoves = 0;
    
        _currentLevel = level;
        
        SwitchTurn(PieceColour.White);
    }

    public PieceColour CurrentTurn()
    {
        return _currentTurn;
    }
}
