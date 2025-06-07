using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevCreator : Creator
{
    [Header("SOs")]
    public Input_SO inputSo;
    public PlayerSystem_SO playerSystemSo;
    public Chain_SO chainSo;
    public Pieces_SO piecesSo;
    public AudioClips_SO audioClipsSo;
    public Settings_SO settingsSo;
    
    [Header("Prefabs")]
    public GameObject piecePrefab;
    public GameObject validPositionPrefab;
    public GameObject selectedBackgroundPrefab;
    
    [Header("Current Level")]
    public int currentLevelNumber;

    [Header("Next Level")] 
    public int nextLevelNumber;

    [Header("Controlled by")] 
    public ControlledBy whiteControlledBy;
    public ControlledBy blackControlledBy;

    [Header("Is Puzzle")]
    public bool isPuzzle;
    
    private void Start()
    {
        if (isPuzzle && whiteControlledBy == ControlledBy.Player && blackControlledBy == ControlledBy.Player)
        {
            Debug.LogError("CANNOT HAVE PUZZLE WITH TWO PLAYERS");
            return;
        }
        if (isPuzzle && whiteControlledBy == ControlledBy.AI && blackControlledBy == ControlledBy.AI)
        {
            Debug.LogError("CANNOT HAVE PUZZLE WITH TWO AIs");
            return;
        }
        
        Random.InitState(42);
        
        Camera.main.backgroundColor = Color.black;
        
        foreach (Dependency dependency in _dependencies)
        {
            LevDependency levDependency = (LevDependency)dependency;
            levDependency.GameStart(this);
        }
        
        Transform levelTextTf = GetFirstObjectWithName(AllTagNames.LevelText);
        if (isPuzzle)
        {
            TMP_Text levelText = levelTextTf.GetComponent<TMP_Text>();

            levelText.text = $"Level {currentLevelNumber}";   
        }
        else
        {
            levelTextTf.gameObject.SetActive(false);
        }
    }
    
    public override void CreateDependencies()
    {
        _dependencies.Add(new LevValidMovesSystem());
        _dependencies.Add(new LevAudioSystem());
        _dependencies.Add(new LevChainUISystem());
        _dependencies.Add(new LevCinemachineSystem());
        _dependencies.Add(new LevBlackSystem());
        _dependencies.Add(new LevBoardSystem());
        _dependencies.Add(new LevLevelCompleteUISystem());
        _dependencies.Add(new LevPauseUISystem());
        _dependencies.Add(new LevWhiteSystem());
        _dependencies.Add(new LevRestartLevelSystem());
        _dependencies.Add(new LevGameOverUISystem());
        _dependencies.Add(new LevSideWinsUISystem());
        _dependencies.Add(new LevEndGameSystem());
        _dependencies.Add(new LevTurnSystem());
    }

    private void OnDestroy()
    {
        foreach (Dependency dependency in _dependencies)
        {
            dependency.Clean();
        }
    }
}

public enum ControlledBy
{
    Player,
    AI
}
