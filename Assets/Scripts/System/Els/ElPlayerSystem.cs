using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Here the Player is allowed to move until
/// A: they are captured by an Enemy piece
/// </summary>
public class ElPlayerSystem : ElDependency
{
    private ElGridSystem _gridSystem;
    private ElEnemiesSystem _enemiesSystem;
    private ElCinemachineSystem _cinemachineSystem;
    private ElTimerUISystem _timerUISystem;
    private ElChainUISystem _chainUISystem;
    private ElAudioSystem _audioSystem;
    private ElGameOverUISystem _gameOverUISystem;
    private ElTurnSystem _turnInfoUISystem;
    private ElRoomNumberUISystem _roomNumberUISystem;
    private ElScoreEntryUISystem _scoreEntryUISystem;
    private ElShopSystem _shopSystem;

    private ElPromoUIController _promoUIController;
    
    private LinkedList<Piece> _capturedPieces = new();
    private LinkedListNode<Piece> _currentPiece;
    private Queue<Piece> _movesInThisTurn = new();
    
    private Transform _playerCharacter;
    
    private List<Transform> _validPositionsVisuals = new(64);
    
    private Vector3 _jumpPosition;

    private Animator _playerAnimator;
    private SpriteRenderer _playerSprite;
    
    private float _moveSpeed;
    private float _sinTime;

    private int _roomNumber;

    public enum States
    {
        WaitingForTurn,
        Idle,
        Moving,
        PawnPromo,
        FadeOutBeforeDoorWalk,
        DoorWalk,
        FadeInAfterDoorWalk,
        Captured,
        TimerExpired,
        LevelComplete,
        EndGame
    }

    private Piece _currentRoomStartPiece;
    
    private States _state;
    
    private Vector3 _doorPositionOnOut;
    private int _roomNumberOnOut;

    private float _timeAtFadeIn;
    private float _fadeInAnimTimer;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElGridSystem gridSystem))
        {
            _gridSystem = gridSystem;
        }
        if (Creator.TryGetDependency(out ElEnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (Creator.TryGetDependency(out ElCinemachineSystem cinemachineSystem))
        {
            _cinemachineSystem = cinemachineSystem;
        }
        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.TryGetDependency(out ElChainUISystem chainUISystem))
        {
            _chainUISystem = chainUISystem;
        }
        if (Creator.TryGetDependency(out ElAudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        if (Creator.TryGetDependency(out ElGameOverUISystem gameOverUISystem))
        {
            _gameOverUISystem = gameOverUISystem;
        }
        if (Creator.TryGetDependency(out ElTurnSystem turnSystem))
        {
            _turnInfoUISystem = turnSystem;
        }
        if (Creator.TryGetDependency(out ElRoomNumberUISystem roomNumberUISystem))
        {
            _roomNumberUISystem = roomNumberUISystem;
        }
        if (Creator.TryGetDependency(out ElScoreEntryUISystem scoreEntryUISystem))
        {
            _scoreEntryUISystem = scoreEntryUISystem;
        }
        if (Creator.TryGetDependency(out ElShopSystem shopSystem))
        {
            _shopSystem = shopSystem;
        }
        
        _currentRoomStartPiece = Creator.playerSystemSo.startingPiece;
        _roomNumber = Creator.playerSystemSo.roomNumberSaved;
        
        Transform playerSpawnPosition = GameObject.FindWithTag("PlayerSpawnPosition").transform;
        Vector3 spawnPos = playerSpawnPosition.position;
        if (Creator.playerSystemSo.levelNumberSaved > 0 || Creator.playerSystemSo.roomNumberSaved > 0)
        {
            spawnPos.x = Creator.playerSystemSo.xValueToStartOn;
        }
        spawnPos += new Vector3(0, Creator.playerSystemSo.roomNumberSaved * 11f, 0);
        
        _playerCharacter = Creator.InstantiateGameObject(Creator.playerPrefab, spawnPos, Quaternion.identity).transform;
        _promoUIController = new ElPromoUIController();
        _promoUIController.GameStart(elCreator);
        _promoUIController.Initialise(_playerCharacter);
        
        _playerAnimator = _playerCharacter.GetComponentInChildren<Animator>();
        
        _jumpPosition = _playerCharacter.position;

        foreach (Animator playerAnimator in _playerCharacter.GetComponentsInChildren<Animator>())
        {
            if (playerAnimator.CompareTag("PlayerVisual"))
            {
                _playerAnimator = playerAnimator;
                _playerSprite = playerAnimator.GetComponent<SpriteRenderer>();
            }
        }
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                Creator.InstantiateGameObject(Creator.validPositionPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }

        _capturedPieces.AddFirst(_currentRoomStartPiece);

        if (Creator.playerSystemSo.levelNumberSaved > 0 && Creator.playerSystemSo.roomNumberSaved == 0 && Creator.timerSo.timePenaltyOnReload == 0)
        {
            UpdateSprite(_currentRoomStartPiece);
            TriggerFadeInAnimation();
            SetState(States.FadeInAfterDoorWalk);
        }
        else
        {
            SetState(States.Idle);
        }
    }

    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }
    
    public override void GameUpdate(float dt)
    {
        HideAllValidMoves();
        
        UpdateValidMoves();
        
        if(Creator.mainMenuSo.isOtherMainMenuCanvasShowing)
            return;
        
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.Idle:
                if(_movesInThisTurn.Count == 0) 
                    break;
                
                if (_movesInThisTurn.Peek() == Piece.Pawn)
                {
                    Vector3 posInFront = _playerCharacter.position + new Vector3(0, 1, 0);
                    
                    if(!_gridSystem.IsPositionValid(posInFront) || _gridSystem.TryGetSingleDoorPosition(posInFront, out SingleDoorPosition checkDoorOpen))
                    {
                        _promoUIController.Show();
                        
                        _movesInThisTurn.Dequeue();
                        SetState(States.PawnPromo);
                        break;
                    }
                    
                    if(_enemiesSystem.IsEnemyAtThisPosition(posInFront))
                    {
                        Vector3 posOnSideLeft = _playerCharacter.position + new Vector3(-1, 1, 0);
                        bool enemyOnLeft = _enemiesSystem.IsEnemyAtThisPosition(posOnSideLeft);
                        Vector3 posOnSideRight = _playerCharacter.position + new Vector3(1, 1, 0);
                        bool enemyOnRight = _enemiesSystem.IsEnemyAtThisPosition(posOnSideRight);

                        if (!enemyOnLeft && !enemyOnRight)
                        {
                            TriggerJumpAnimation();

                            _movesInThisTurn.Dequeue();

                            if (_movesInThisTurn.Count > 0)
                            {
                                //Allow player to make another move
                                UpdateSprite(_movesInThisTurn.Peek());
                                SetState(States.Idle);
                                _chainUISystem.HighlightNextPiece();
                            }
                            else
                            {
                                SetState(States.WaitingForTurn);
                                _turnInfoUISystem.SwitchTurn(ElTurnSystem.Turn.Enemy);
                            }
                            break;
                        }
                    }
                }
                
                if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame())
                {
                    UpdatePlayerPosition();
                }
                break;
            case States.Moving:
                if (_playerCharacter.position != _jumpPosition)
                {
                    _sinTime += dt * _moveSpeed;
                    _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
                    float t = Evaluate(_sinTime);
                    _playerCharacter.position = Vector3.Lerp(_playerCharacter.position, _jumpPosition, t);
                    break;
                }
                
                if (_playerCharacter.position == _jumpPosition)
                {
                    _playerCharacter.position = new Vector3(((int)_playerCharacter.position.x) + 0.5f, ((int)_playerCharacter.position.y) + 0.5f, 0);
                    _sinTime = 0;
                    
                    //If Capture enemies exist, then we need to check the position the player has moved to is safe. If not, then it's game over for the player!
                    if (_enemiesSystem.CheckIfEnemiesCanCapture())
                    {
                        //Game over!!!
                        SetState(States.WaitingForTurn);
                        return;
                    }

                    if (_gridSystem.TryGetSingleDoorPosition(_playerCharacter.position, out SingleDoorPosition singleDoorPosition))
                    {
                        if (singleDoorPosition.isDoorOpen)
                        {
                            _timerUISystem.ResetTimerChangedAmount(true);
                            _timerUISystem.StopTimer();
                            Creator.playerSystemSo.xValueToStartOn = singleDoorPosition.transform.position.x;
                            Creator.playerSystemSo.startingPiece = _currentPiece.Value;
                            if (singleDoorPosition.isFinalDoor)
                            {
                                TriggerFadeOutAnimation();
                                SetState(States.LevelComplete);
                            }
                            else
                            {
                                _doorPositionOnOut = singleDoorPosition.GetPlayerPositionOnOut();
                                _roomNumberOnOut = singleDoorPosition.GetOtherDoorRoomNumber();
                                _roomNumber = _roomNumberOnOut;
                                Creator.playerSystemSo.roomNumberSaved = _roomNumberOnOut;
                                _moveSpeed = Creator.playerSystemSo.walkThroughDoorSpeed;
                                TriggerFadeOutAnimation();
                                SetState(States.FadeOutBeforeDoorWalk);
                            }
                            break;
                        }
                    }
                    else if (_enemiesSystem.TryGetEnemyAtPosition(_playerCharacter.position,
                                 out ElEnemyController enemyController))
                    {
                        //Add this player to the 'captured pieces' list
                        Piece enemyPiece = enemyController.GetPiece();
                        
                        _timerUISystem.AddTime(Creator.timerSo.capturePieceTimeAdd[enemyPiece]);
                        
                        _capturedPieces.AddLast(enemyPiece);
                        _movesInThisTurn.Enqueue(enemyPiece);
                        _enemiesSystem.PieceCaptured(enemyController, GetRoomNumber());
                    }
                    else
                    {
                        //Player has not captured an enemy so we must reset the multiplier
                        _timerUISystem.ResetTimerChangedAmount(false);
                    }
                    
                    //IF the player is a pawn, we want to check what's directly in front of the player.
                    //IF it is an invalid position OR a door, then the player has a pawn and is at the end of the room. Therefore enable promotion
                    if (_currentPiece.Value == Piece.Pawn)
                    {
                        Vector3 posInFront = _playerCharacter.position + new Vector3(0, 1, 0);
                        if (!_gridSystem.IsPositionValid(posInFront) ||
                            _gridSystem.TryGetSingleDoorPosition(posInFront, out SingleDoorPosition foo))
                        {
                            _promoUIController.Show();
                            SetState(States.PawnPromo);
                            break;
                        }
                    }
                    
                    if (GetRoomNumber() == Creator.shopSo.shopRoomNumber)
                    {
                        _shopSystem.TryGetArtefactAtPosition(_playerCharacter.position);
                        _shopSystem.TryGetUpgradeAtPosition(_playerCharacter.position);
                        
                        if (_shopSystem.TryGetShopPieceAtPosition(_playerCharacter.position, out Piece piece))
                        {
                            _currentPiece.Value = piece;
                            _chainUISystem.NewRoomClearChain();
                            _chainUISystem.ShowNewPiece(piece, true);
                            _currentRoomStartPiece = piece;
                            SetState(States.Idle);
                            break;
                        }
                    }
                    _currentPiece = _currentPiece.Next;
                    if (_movesInThisTurn.Count > 0)
                    {
                        //Allow player to make another move
                        UpdateSprite(_movesInThisTurn.Peek());
                        SetState(States.Idle);
                        _chainUISystem.HighlightNextPiece();
                    }
                    else
                    {
                        SetState(States.WaitingForTurn);
                        _turnInfoUISystem.SwitchTurn(ElTurnSystem.Turn.Enemy);
                    }
                }
                break;
            case States.PawnPromo:
                if (_promoUIController.IsPromoChosen())
                {
                    Piece pieceChosen = _promoUIController.PieceChosen();
                    _currentPiece.Value = pieceChosen;
                    
                    int index = 0;
                    LinkedListNode<Piece> temp = _capturedPieces.First;
                    while (temp != null)
                    {
                        if(temp == _currentPiece)
                            break;

                        index++;
                        temp = temp.Next;
                    }

                    index *= 2; //Have to double index for the chainUI as for every other index, there is an arrow sprite which we NEVER want to change
                    _chainUISystem.PawnPromoted(index, pieceChosen);
                    
                    _promoUIController.Hide();
                    
                    _currentPiece = _currentPiece.Next;
                    
                    if (_movesInThisTurn.Count > 0)
                    {
                        //Allow player to make another move
                        UpdateSprite(_movesInThisTurn.Peek());
                        SetState(States.Idle);
                        _chainUISystem.HighlightNextPiece();
                    }
                    else
                    {
                        UpdateSprite(pieceChosen);
                        SetState(States.WaitingForTurn);
                        _turnInfoUISystem.SwitchTurn(ElTurnSystem.Turn.Enemy);
                    }
                }
                break;
            case States.FadeOutBeforeDoorWalk:
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _cinemachineSystem.SwitchState(_roomNumberOnOut);
                    _roomNumberUISystem.UpdateRoomNumberText();
                    
                    _currentRoomStartPiece = _currentPiece.Value;
                    _chainUISystem.NewRoomClearChain();
                    _chainUISystem.ResetPosition();
                    _chainUISystem.ShowNewPiece(_currentRoomStartPiece, true);
                    SetState(States.DoorWalk);
                }
                break;
            case States.DoorWalk:
                if (_playerCharacter.position != _doorPositionOnOut)
                {
                    _sinTime += dt * _moveSpeed;
                    _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
                    float t = Evaluate(_sinTime);
                    _playerCharacter.position = Vector3.Lerp(_playerCharacter.position, _doorPositionOnOut, t);
                }
                
                if (_playerCharacter.position == _doorPositionOnOut)
                {
                    _playerCharacter.position = new Vector3(((int)_playerCharacter.position.x) + 0.5f, ((int)_playerCharacter.position.y) + 0.5f, 0);
                    _sinTime = 0;

                    TriggerFadeInAnimation();
                    _fadeInAnimTimer = dt;
                    _timeAtFadeIn = dt;
                    
                    SetState(States.FadeInAfterDoorWalk);
                }
                break;
            case States.FadeInAfterDoorWalk:
                //Fade in animation is 1 second
                if (_fadeInAnimTimer >= _timeAtFadeIn + 1)
                {
                    //Chain resets when player goes into new room
                    _movesInThisTurn.Clear();
                    _capturedPieces.Clear();
                    _capturedPieces.AddFirst(_currentRoomStartPiece);
                    
                    SetState(States.Idle);
                }
                else
                {
                    _fadeInAnimTimer += dt;
                }
                break;
            case States.Captured:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _timerUISystem.ResetTimerChangedAmount(false);
                    SetState(States.EndGame);
                    _gameOverUISystem.Show("Captured", true);
                }
                break;
            case States.TimerExpired:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _timerUISystem.ResetTimerChangedAmount(false);
                    if (AuthenticationService.Instance is not null)
                    {
                        if (AuthenticationService.Instance.IsSignedIn)
                        {
                            CheckScore();
                        }
                        else
                        {
                            _gameOverUISystem.Show("Timer Expired", false);
                        }
                    }
                    else
                    {
                        _gameOverUISystem.Show("Timer Expired", false);
                    }
                    SetState(States.EndGame);
                }
                break;
            case States.LevelComplete:
                //Wait 1 second before we load the next level
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    Creator.shopSo.ResetData();
                    Creator.enemySo.ResetData();
                    Creator.playerSystemSo.levelNumberSaved++;
                    Creator.playerSystemSo.roomNumberSaved = 0;
                    Creator.playerSystemSo.startingPiece = _currentPiece.Value;
                    
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                break;
            case States.EndGame:
                break;
        }
    }

    private async void CheckScore()
    {
        //First we add the player's score
        //Score examples:
        //Level 1 Room 7 = Score of 7
        //Level 2 Room 1 = Score of 9
        double score = Creator.playerSystemSo.levelNumberSaved * 8 + Creator.playerSystemSo.roomNumberSaved + 1;
        
        LeaderboardScoresPage topScores = await LeaderboardsService.Instance.GetScoresAsync(Creator.scoreboardSo.ScoreboardID, new GetScoresOptions(){Offset = 0, Limit = 11});
        //If there are less than 10 entries then we guarantee the player makes it onto the scoreboard
        if (topScores.Results.Count < 10)
        {
            try
            {
                LeaderboardEntry leaderboardEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(Creator.scoreboardSo.ScoreboardID);
                //Only need the player to enter their score if it is better than their previous score
                if (leaderboardEntry.Score < score)
                {
                    _scoreEntryUISystem.Show(score);
                }
                else
                {
                    _gameOverUISystem.Show("Timer Expired", false);
                }
            }
            catch (Exception e)
            {
                _scoreEntryUISystem.Show(score);
            }
        }
        else
        {
            LeaderboardEntry lowestEntry = topScores.Results[^1];
        
            if (lowestEntry.Score < score)
            {
                _scoreEntryUISystem.Show(score);
            }
            else
            {
                _gameOverUISystem.Show("Timer Expired", false);
            }
        }
    }

    private void CheckDefiniteMoves(Piece piece, List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position + pieceMove;
            
            if(positionRequested != newPos) continue;
            
            if (_gridSystem.TryGetSingleDoorPosition(newPos, out SingleDoorPosition singleDoorPosition))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPosition.isDoorOpen) continue;
            }
            else if(!_gridSystem.IsPositionValid(newPos))
                continue;
            
            if (piece == Piece.Pawn && _enemiesSystem.IsEnemyAtThisPosition(newPos) && pieceMove == new Vector3(0, 1, 0))
            {
                //Cannot take a piece that is directly in front of pawn
                continue;
            }
            
            if (!Creator.playerSystemSo.firstMoveMadeWhileShowingMainMenu)
            {
                if(SceneManager.sceneCount > 1)
                    SceneManager.UnloadSceneAsync("MainMenuScene");
                Creator.playerSystemSo.firstMoveMadeWhileShowingMainMenu = true;
            }
            
            _jumpPosition = positionRequested;
            _moveSpeed = Creator.playerSystemSo.moveSpeed;
            _timerUISystem.StartTimer();
            SetState(States.Moving);
            
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSfx(1);
            
            _movesInThisTurn.Dequeue();
        }
    }

    private void CheckIndefiniteMoves(List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        //Here we check all possible positions of the player's current piece
        //If we find a possible position that is equal to the position requested, we move the player to this piece and break out of the loop
        //When using a pieceMove, if we run into an invalid position or a closed door, we have explored that direction as far as possible 
        //So we get the next pieceMove or the loop finishes
        
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position;
            bool foundSpot = false;

            while (true)
            {
                Vector3 nextSpot = newPos + pieceMove;
                
                //If we have found a closed door, we have explored this path the furthest
                if (_gridSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition singleDoorPosition))
                {
                    //If this door is locked, we cannot allow access
                    //canMoveToNextSpot = canMoveToNextSpot && singleDoorPosition.isDoorOpen;
                    if(!singleDoorPosition.isDoorOpen) break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                //If we have found an invalid position, we have explored this path the furthest
                
                //If, while exploring this path we find an enemy and the position of that enemy is NOT the position requested,
                //then it is impossible for the player to choose a position past this enemy.
                //then we have explored this path the furthest
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out ElEnemyController eneCont))
                {
                    if(eneCont.GetPosition() != positionRequested) break;
                }
                
                
                //If the position requested isn't nextSpot, then we haven't found the position.
                //So we must continue the search
                if (positionRequested != nextSpot)
                {
                    newPos = nextSpot;
                    continue;
                }
                    
                if (!Creator.playerSystemSo.firstMoveMadeWhileShowingMainMenu)
                {
                    if(SceneManager.sceneCount > 1)
                        SceneManager.UnloadSceneAsync("MainMenuScene");
                    Creator.playerSystemSo.firstMoveMadeWhileShowingMainMenu = true;
                }
                
                _jumpPosition = positionRequested;
                _moveSpeed = Creator.playerSystemSo.moveSpeed;
                _timerUISystem.StartTimer();
                SetState(States.Moving);
                
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSfx(1);

                _movesInThisTurn.Dequeue();
                
                foundSpot = true;
                break;
            }
            if(foundSpot)
                break;
        }
    }
    
    private void UpdatePlayerPosition()
    {
        Vector3 positionRequested = _gridSystem.GetHighlightPosition();
        
        //Depending on the piece, we allow the player an additional move (player will always start with a king move)
        Piece piece = _movesInThisTurn.Peek();
        
        switch (piece)
        {
            case Piece.Pawn:
                //Allow player to move up one space, if that space is available
                List<Vector3> pawnMoves = new();
                
                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + defaultMove))
                {
                    pawnMoves.Add(defaultMove);
                }
                
                //Check if there is an enemy to the top left or top right. If so, these moves are allowed
                Vector3 topLeft = new Vector3(-1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topLeft))
                {
                    pawnMoves.Add(topLeft);
                }
                
                Vector3 topRight = new Vector3(1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topRight))
                {
                    pawnMoves.Add(topRight);
                }
                
                CheckDefiniteMoves(piece, pawnMoves, positionRequested);
                break;
            case Piece.Rook:
                List<Vector3> rookMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0)
                };
                
                CheckIndefiniteMoves(rookMoves, positionRequested);
                break;
            case Piece.Knight:
                List<Vector3> knightMoves = new()
                {
                    new Vector3(1, 2, 0),
                    new Vector3(-1, 2, 0),
                    new Vector3(1, -2, 0),
                    new Vector3(-1, -2, 0),
                    new Vector3(-2, 1, 0),
                    new Vector3(-2, -1, 0),
                    new Vector3(2, 1, 0),
                    new Vector3(2, -1, 0)
                };
                
                CheckDefiniteMoves(piece, knightMoves, positionRequested);
                break;
            case Piece.Bishop:
                List<Vector3> bishopMoves = new()
                {
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                CheckIndefiniteMoves(bishopMoves, positionRequested);
                break;
            case Piece.Queen:
                List<Vector3> queenMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                CheckIndefiniteMoves(queenMoves, positionRequested);
                break;
            case Piece.King:
                List<Vector3> kingMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                CheckDefiniteMoves(piece, kingMoves, positionRequested);
                break;
        }
    }
    
    private List<Vector3> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _playerCharacter.position + move;
                    
            if (_gridSystem.TryGetSingleDoorPosition(positionFromPlayer, out SingleDoorPosition singleDoorPos))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPos.isDoorOpen) continue;
            }
            else if(!_gridSystem.IsPositionValid(positionFromPlayer))
                continue;
            
            validMoves.Add(positionFromPlayer);
        }

        return validMoves;
    }

    private List<Vector3> CheckValidIndefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 diagonal in moves)
        {
            Vector3 furthestPointOfDiagonal = _playerCharacter.position;
            while (true)
            {
                Vector3 nextSpot = furthestPointOfDiagonal + diagonal;
                if (_gridSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition knightSingDoorPos))
                {
                    //If this door is locked, we cannot allow access
                    if(!knightSingDoorPos.isDoorOpen) 
                        break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                
                furthestPointOfDiagonal = nextSpot;
                validMoves.Add(furthestPointOfDiagonal);
                        
                if(_enemiesSystem.IsEnemyAtThisPosition(nextSpot)) break;
            }
        }

        return validMoves;
    }

    private void UpdateValidMoves()
    {
        if(_state != States.Idle || _movesInThisTurn.Count == 0) return;
        
        Piece piece = _movesInThisTurn.Peek();
        List<Vector3> validMoves = new();
        switch (piece)
        {
            case Piece.Pawn:
                List<Vector3> pawnMoves = new();

                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + defaultMove))
                {
                    pawnMoves.Add(defaultMove);
                }
                Vector3 topLeft = new Vector3(-1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topLeft))
                {
                    pawnMoves.Add(topLeft);
                }
                Vector3 topRight = new Vector3(1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topRight))
                {
                    pawnMoves.Add(topRight);
                }

                List<Vector3> pawnValidMoves = CheckValidDefiniteMoves(pawnMoves);
                validMoves = validMoves.Concat(pawnValidMoves).ToList();
                break;
            case Piece.Rook:
                List<Vector3> rookMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0)
                };

                List<Vector3> rookValidMoves = CheckValidIndefiniteMoves(rookMoves);
                validMoves = validMoves.Concat(rookValidMoves).ToList();
                break;
            case Piece.Knight:
                List<Vector3> knightMoves = new()
                {
                    new Vector3(1, 2, 0),
                    new Vector3(-1, 2, 0),
                    new Vector3(1, -2, 0),
                    new Vector3(-1, -2, 0),
                    new Vector3(-2, 1, 0),
                    new Vector3(-2, -1, 0),
                    new Vector3(2, 1, 0),
                    new Vector3(2, -1, 0)
                };
                
                List<Vector3> knightValidMoves = CheckValidDefiniteMoves(knightMoves);
                validMoves = validMoves.Concat(knightValidMoves).ToList();
                break;
            case Piece.Bishop:
                List<Vector3> bishopMoves = new()
                {
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                List<Vector3> bishopValidMoves = CheckValidIndefiniteMoves(bishopMoves);
                validMoves = validMoves.Concat(bishopValidMoves).ToList();
                break;
            case Piece.Queen:
                List<Vector3> queenMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                List<Vector3> queenValidMoves = CheckValidIndefiniteMoves(queenMoves);
                validMoves = validMoves.Concat(queenValidMoves).ToList();
                break;
            case Piece.King:
                List<Vector3> kingMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                List<Vector3> kingValidMoves = CheckValidDefiniteMoves(kingMoves);
                validMoves = validMoves.Concat(kingValidMoves).ToList();
                break;
        }

        for (int i = 0; i < validMoves.Count; i++)
        {
            _validPositionsVisuals[i].position = validMoves[i];
            _validPositionsVisuals[i].gameObject.SetActive(true);
        }
    }
    
    private void HideAllValidMoves()
    {
        foreach (Transform validPositionsVisual in _validPositionsVisuals)
        {
            validPositionsVisual.gameObject.SetActive(false);
        }
    }

    private void UpdateSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.NotChosen:
                _playerSprite.sprite = default;
                break;
            case Piece.Pawn:
                _playerSprite.sprite = Creator.playerSystemSo.pawn;
                break;
            case Piece.Rook:
                _playerSprite.sprite = Creator.playerSystemSo.rook;
                break;
            case Piece.Knight:
                _playerSprite.sprite = Creator.playerSystemSo.knight;
                break;
            case Piece.Bishop:
                _playerSprite.sprite = Creator.playerSystemSo.bishop;
                break;
            case Piece.Queen:
                _playerSprite.sprite = Creator.playerSystemSo.queen;
                break;
            case Piece.King:
                _playerSprite.sprite = Creator.playerSystemSo.king;
                break;
        }
    }
    
    private void TriggerJumpAnimation()
    {
        _playerAnimator.SetTrigger("Jump");
    }
    
    private void TriggerFadeOutAnimation()
    {
        _playerAnimator.SetTrigger("FadeOut");
    }

    private void TriggerFadeInAnimation()
    {
        _playerAnimator.SetTrigger("FadeIn");
    }
    
    public void SetState(States state)
    {
        switch (state)
        {
            case States.Captured:
                _audioSystem.PlayCapturedByEnemySfx();
                TriggerFadeOutAnimation();
                break;
            case States.TimerExpired:
                TriggerFadeOutAnimation();
                break;
            case States.Idle:
                if (_movesInThisTurn.Count == 0)
                {
                    UpdateSprite(_currentRoomStartPiece);
                    //Reset possible moves
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Enqueue(capturedPiece);
                    }
                    _currentPiece = _capturedPieces.First;
                    _chainUISystem.ResetPosition();
                }
                break;
        }
        _state = state;
    }

    public States GetState()
    {
        return _state;
    }
    
    public Vector3 GetPlayerPosition()
    {
        return _playerCharacter.position;
    }

    public int GetRoomNumber()
    {
        return _roomNumber;
    }
}
