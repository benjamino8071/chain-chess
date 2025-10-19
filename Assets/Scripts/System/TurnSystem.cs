using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private UISystem _uiSystem;
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
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _playerSetTilesSystem = creator.GetDependency<PlayerSetTilesSystem>();
        
        LoadInitialLevel();
    }

    public void LoadInitialLevel()
    {
        Level level = Creator.levelsSo.GetLevel(Creator.saveDataSo.sectionLastLoaded, Creator.saveDataSo.levelLastLoaded);
        LoadLevelRuntime(level, false);
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        if (_endGameSystem.isEndGame)
        {
            return;
        }
        
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
                Creator.statsTurns++;
                //HERE
                
                List<ValidMove> validMoves = _whiteSystem.playerController.GetAllValidMovesOfFirstPiece();
                if (validMoves.Count == 0)
                {
                    _whiteSystem.playerController.SetState(PieceState.Blocked);
                }
                else
                {
                    _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfFirstPiece());
                
                    _whiteSystem.playerController.SetState(PieceState.FindingMove);
                    _blackSystem.SetStateForAllPieces(PieceState.WaitingForTurn);

                    List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                    foreach (UIChain uiChain in uiChains)
                    {
                        uiChain.ShowChain(_whiteSystem.playerController);
                    }
                }
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
    
    public void LoadLevelRuntime(Level level, bool showChain = true)
    {
        Random.InitState(42);
       
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _playerSetTilesSystem.HideAll();
        
        List<UICurrentLevelMenu> uiCurrentLevelMenus = _uiSystem.GetUI<UICurrentLevelMenu>();
        foreach (UICurrentLevelMenu uiCurrentLevelMenu in uiCurrentLevelMenus)
        {
            uiCurrentLevelMenu.SetLevelsButtonText(level);
        }
        
        _whiteSystem.SpawnPieces(level);
        _whiteSystem.SetLost(false);
        
        _blackSystem.SpawnPieces(level);
        _blackSystem.SetLost(false);

        _boardSystem.ShowTapPoint(_whiteSystem.playerController.piecePos);
        
        _endGameSystem.ResetEndGame();

        Creator.statsTurns = 0;
        Creator.statsMoves = 0;
        Creator.statsCaptures = 0;
    
        _currentLevel = level;

        Creator.saveDataSo.sectionLastLoaded = level.section;
        Creator.saveDataSo.levelLastLoaded = level.level;
        Creator.SaveToDisk();

        if (showChain)
        {
            _uiSystem.ShowRightTopSideUI(AllTagNames.UIChain);
        }
        
        SwitchTurn(PieceColour.White);
    }

    public PieceColour CurrentTurn()
    {
        return _currentTurn;
    }
}
