using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreMountains.Feedbacks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    private ElDoorsSystem _doorsSystem;
    private ElXPBarUISystem _xpBarUISystem;
    private ElUpgradeUISystem _upgradeUISystem;

    private ElPromoUIController _promoUIController;
    
    private LinkedList<Piece> _capturedPieces = new();
    private LinkedListNode<Piece> _currentPiece;
    
    private Transform _playerCharacter;
    
    private List<Transform> _validPositionsVisuals = new(64);
    
    private Vector3 _jumpPosition;

    private Animator _playerAnimator;
    private SpriteRenderer _playerSprite;
    private MMF_Player _pieceCapturedPlayer;
    
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
        DestroyingAllEnemiesInRoom,
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

    private float _destroyEnemyTimer;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _gridSystem = elCreator.GetDependency<ElGridSystem>();
        _enemiesSystem = elCreator.GetDependency<ElEnemiesSystem>();
        _cinemachineSystem = elCreator.GetDependency<ElCinemachineSystem>();
        _timerUISystem = elCreator.GetDependency<ElTimerUISystem>();
        _chainUISystem = elCreator.GetDependency<ElChainUISystem>();
        _audioSystem = elCreator.GetDependency<ElAudioSystem>();
        _gameOverUISystem = elCreator.GetDependency<ElGameOverUISystem>();
        _turnInfoUISystem = elCreator.GetDependency<ElTurnSystem>();
        _roomNumberUISystem = elCreator.GetDependency<ElRoomNumberUISystem>();
        _scoreEntryUISystem = elCreator.GetDependency<ElScoreEntryUISystem>();
        _shopSystem = elCreator.GetDependency<ElShopSystem>();
        _doorsSystem = elCreator.GetDependency<ElDoorsSystem>();
        _xpBarUISystem = elCreator.GetDependency<ElXPBarUISystem>();
        _upgradeUISystem = elCreator.GetDependency<ElUpgradeUISystem>();
        
        _currentRoomStartPiece = Creator.playerSystemSo.startingPiece;
        _roomNumber = Creator.playerSystemSo.roomNumberSaved;

        Transform playerSpawnPosition = elCreator.GetFirstObjectWithName(AllTagNames.PlayerSpawnPosition);
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

        _pieceCapturedPlayer = _playerCharacter.GetComponentInChildren<MMF_Player>();
        
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
        
        Creator.playerSystemSo.moveMadeInNewRoom = false;

        if (Creator.playerSystemSo.levelNumberSaved > 0 && Creator.playerSystemSo.roomNumberSaved == 0)
        {
            UpdateSprite(_currentRoomStartPiece);
            TriggerFadeInAnimation();
            SetState(States.FadeInAfterDoorWalk);
        }
        else
        {
            if (Creator.playerSystemSo.levelNumberSaved > 0)
            {
                Creator.CloseFakeDoors();
            }
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
        
        if(_upgradeUISystem.ChoosingUpgrade())
            return;
        
        UpdateValidMoves();
        
        if(Creator.mainMenuSo.isOtherMainMenuCanvasShowing)
            return;
        
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.Idle:
                if(_currentPiece is null) 
                    break;
                
                if (_currentPiece.Value == Piece.Pawn)
                {
                    Vector3 posInFront = _playerCharacter.position + new Vector3(0, 1, 0);
                    
                    if(!_gridSystem.IsPositionValid(posInFront) || _gridSystem.TryGetSingleDoorPosition(posInFront, out SingleDoorPosition checkDoorOpen))
                    {
                        _promoUIController.Show();

                        _chainUISystem.UpdateMovesRemainingText(0);
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

                            _chainUISystem.UpdateMovesRemainingText(0);

                            _currentPiece = _currentPiece.Next;
                            
                            if (_currentPiece is not null)
                            {
                                //Allow player to make another move
                                UpdateSprite(_currentPiece.Value);
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
                            //_timerUISystem.ResetTimerChangedAmount(true);
                            //_timerUISystem.StopTimer();
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
                    
                    _chainUISystem.UpdateMovesRemainingText(0);
                    if (_enemiesSystem.TryGetEnemyAtPosition(_playerCharacter.position,
                                 out ElEnemyController enemyController))
                    {
                        //Add this player to the 'captured pieces' list
                        Piece enemyPiece = enemyController.GetPiece();
                        
                        _enemiesSystem.PieceCaptured(enemyController);
                        
                        bool enemiesCleared = _enemiesSystem.IsEnemiesInRoomCleared(GetRoomNumber());
                        _xpBarUISystem.IncreaseProgressBar(Creator.xpBarSo.capturePieceXPGain[enemyPiece], !enemiesCleared);
                        if (_currentPiece.Next is not null && Creator.playerSystemSo.artefacts.Contains(ArtefactTypes.UseCapturedPieceStraightAway))
                        {
                            //We add this piece to the position in the queue between the current piece, and the next piece.
                            LinkedListNode<Piece> pieceNode = new LinkedListNode<Piece>(enemyPiece);
                            _capturedPieces.AddAfter(_currentPiece, pieceNode);
                            int index = 0;
                            LinkedListNode<Piece> temp = _capturedPieces.First;
                            while (temp != null)
                            {
                                index++;
                                if(temp == _currentPiece)
                                    break;

                                temp = temp.Next;
                            }

                            index *= 2; //Have to double index for the chainUI as for every other index, there is an arrow sprite which we NEVER want to change
                            
                            _chainUISystem.PieceSandwiched(index, enemyPiece);
                        }
                        else
                        {
                            _capturedPieces.AddLast(enemyPiece);
                            _chainUISystem.ShowNewPiece(enemyController.GetPiece());
                        }
                        _chainUISystem.UpdateMovesRemainingText(0);
                        
                        if (enemiesCleared)
                        {
                            _audioSystem.PlayRoomCompleteSfx();
                            //_timerUISystem.StopTimer();
                            _doorsSystem.SetRoomDoorsOpen(GetRoomNumber());
                        }
                        else if (enemyPiece == Piece.King &&
                                 Creator.playerSystemSo.artefacts.Contains(ArtefactTypes.CaptureKingClearRoom))
                        {
                            //_timerUISystem.StopTimer();
                            _destroyEnemyTimer = 0.19f;
                            SetState(States.DestroyingAllEnemiesInRoom);
                            return;
                        }
                    }
                    else
                    {
                        //Player has not captured an enemy so we must reset the multiplier
                        _xpBarUISystem.ResetMultiplier();
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
                            _audioSystem.PlayerLevelUpSfx();
                            SetState(States.Idle);
                            break;
                        }
                    }
                    _currentPiece = _currentPiece.Next;
                    
                    if (_currentPiece is not null)
                    {
                        //Allow player to make another move
                        UpdateSprite(_currentPiece.Value);
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
                    _audioSystem.PlayerLevelUpSfx();
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
                    
                    if (_currentPiece is not null)
                    {
                        //Allow player to make another move
                        UpdateSprite(_currentPiece.Value);
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
                    Creator.playerSystemSo.moveMadeInNewRoom = false;
                    Creator.xpBarSo.levelNumberOnRoomEnter = Creator.xpBarSo.levelNumber;
                    _currentPiece = null;
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
                    _capturedPieces.Clear();
                    _capturedPieces.AddFirst(_currentRoomStartPiece);
                    _audioSystem.PlayDoorClosedSfx();
                    _doorsSystem.SetRoomDoorsClosed(GetRoomNumber());
                    SetState(States.Idle);
                }
                else
                {
                    _fadeInAnimTimer += dt;
                }
                break;
            case States.DestroyingAllEnemiesInRoom:
                if (_destroyEnemyTimer > 0)
                {
                    _destroyEnemyTimer -= dt;
                    if (_destroyEnemyTimer <= 0)
                    {
                        List<ElEnemyController> enemiesInRoom = _enemiesSystem.GetEnemiesInRoom(GetRoomNumber());
                        Piece piece = enemiesInRoom[0].GetPiece();
                        _enemiesSystem.PieceCaptured(enemiesInRoom[0]);
                        
                        bool enemiesCleared = _enemiesSystem.IsEnemiesInRoomCleared(GetRoomNumber());
                        _xpBarUISystem.IncreaseProgressBar(Creator.xpBarSo.capturePieceXPGain[piece], !enemiesCleared);
                            
                        if (enemiesCleared)
                        {
                            Debug.Log("This message should appear after ALL enemies in room are destroyed");
                            _audioSystem.PlayRoomCompleteSfx();
                            _doorsSystem.SetRoomDoorsOpen(GetRoomNumber());
                    
                            _currentPiece = _currentPiece.Next;
                    
                            if (_currentPiece is not null)
                            {
                                //Allow player to make another move
                                UpdateSprite(_currentPiece.Value);
                                SetState(States.Idle);
                                _chainUISystem.HighlightNextPiece();
                            }
                            else
                            {
                                SetState(States.WaitingForTurn);
                                _turnInfoUISystem.SwitchTurn(ElTurnSystem.Turn.Enemy);
                            }
                        }
                        else
                        {
                            _destroyEnemyTimer = 0.19f;
                        }
                    }
                }
                break;
            case States.Captured:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _xpBarUISystem.ResetMultiplier();
                    SetState(States.EndGame);
                    _gameOverUISystem.Show("Captured", true);
                }
                break;
            case States.TimerExpired:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _xpBarUISystem.ResetMultiplier();
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
        
        try
        {

            LeaderboardScoresPage topScores =
                await LeaderboardsService.Instance.GetScoresAsync(Creator.scoreboardSo.ScoreboardID,
                    new GetScoresOptions() { Offset = 0, Limit = 11 });
            //If there are less than 10 entries then we guarantee the player makes it onto the scoreboard
            if (topScores.Results.Count < 10)
            {
                LeaderboardEntry leaderboardEntry =
                    await LeaderboardsService.Instance.GetPlayerScoreAsync(Creator.scoreboardSo.ScoreboardID);
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
        catch (LeaderboardsException e)
        {
            if (e.Reason == LeaderboardsExceptionReason.EntryNotFound)
            {
                try
                {
                    await LeaderboardsService.Instance.AddPlayerScoreAsync(Creator.scoreboardSo.ScoreboardID, score - 1);
                    CheckScore();
                }
                catch (Exception)
                {
                    _gameOverUISystem.Show("Timer Expired", false);
                }
            }
            else
            {
                _gameOverUISystem.Show("Timer Expired", false);
            }
            Debug.Log("Reason: "+e.Reason);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error while checking final score: "+e);
            _gameOverUISystem.Show("Timer Expired", false);
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
            //_timerUISystem.StartTimer();
            Creator.playerSystemSo.moveMadeInNewRoom = true;
            SetState(States.Moving);
            
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSfx(1);
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
                //_timerUISystem.StartTimer();
                Creator.playerSystemSo.moveMadeInNewRoom = true;
                SetState(States.Moving);
                
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSfx(1);
                
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
        Piece piece = _currentPiece.Value;
        
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
        if(_state != States.Idle || _currentPiece is null) return;
        
        Piece piece = _currentPiece.Value;
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

    public void PlayFloatingTextPlayer(float intensity)
    {
        _pieceCapturedPlayer.PlayFeedbacks(_playerCharacter.position, intensity);
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
                if (_currentPiece is null)
                {
                    _currentPiece = _capturedPieces.First;
                    UpdateSprite(_currentPiece.Value);
                    //Reset possible moves
                    _chainUISystem.UpdateMovesRemainingText(_capturedPieces.Count);
                    _chainUISystem.ResetPosition();
                }
                break;
        }
        _state = state;
    }

    public void SetPromoXpGainText()
    {
        _promoUIController.SetPromoXpGainText();
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

    public void EnemyCaptured(ElEnemyController enemyController)
    {
        //Add this player to the 'captured pieces' list
        Piece enemyPiece = enemyController.GetPiece();
        _enemiesSystem.PieceCaptured(enemyController);
        
        bool enemiesCleared = _enemiesSystem.IsEnemiesInRoomCleared(GetRoomNumber()); 
        _xpBarUISystem.IncreaseProgressBar(Creator.xpBarSo.capturePieceXPGain[enemyPiece], !enemiesCleared);
        _capturedPieces.AddLast(enemyPiece);
        _chainUISystem.ShowNewPiece(enemyController.GetPiece());
        _chainUISystem.UpdateMovesRemainingText(0);
                        
        if (enemiesCleared)
        {
            _audioSystem.PlayRoomCompleteSfx();
            //_timerUISystem.StopTimer();
            _doorsSystem.SetRoomDoorsOpen(GetRoomNumber());
        }
        else if (enemyPiece == Piece.King &&
                 Creator.playerSystemSo.artefacts.Contains(ArtefactTypes.CaptureKingClearRoom))
        {
            //_timerUISystem.StopTimer();
            _destroyEnemyTimer = 0.19f;
            SetState(States.DestroyingAllEnemiesInRoom);
        }
    }
}
