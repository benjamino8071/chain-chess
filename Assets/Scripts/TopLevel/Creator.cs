using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Creator : MonoBehaviour
{
    [Title("SOs")]
    public SaveData_SO saveDataSo;
    public Input_SO inputSo;
    public Chain_SO chainSo;
    public Board_SO boardSo;
    public Pieces_SO piecesSo;
    public AudioClips_SO audioClipsSo;
    public Settings_SO settingsSo;
    public Levels_SO levelsSo;
    public LevelComplete_SO levelCompleteSo;
    public GameOver_SO gameOverSo;
    public MiscUI_SO miscUiSo;
    public Rulebook_SO rulebookSo;

    [Title("Prefabs")]
    public GameObject piecePrefab;
    public GameObject validPositionPrefab;
    public GameObject playerSetTilePrefab;
    public GameObject selectedBackgroundPrefab;
    public GameObject imagePrefab;
    public GameObject rulebookHighlightImagePrefab;
    public GameObject blockedTextEffect;

    [Title("Camera")]
    public Camera mainCam;
    
    [Title("Frame Rate")]
    public int frameRate = 120;

    [HideInInspector] public int statsTurns;
    [HideInInspector] public int statsMoves;
    [HideInInspector] public int statsCaptures;
    [HideInInspector] public bool firstInputTaken;
    private DateTime _dateTimeSinceLastSave;

    private List<Dependency> _dependencies = new();
    
    [Title("Transition")]
    
    [SerializeField] private RectTransform sceneTransitionCanvas;
    [SerializeField] private RectTransform sceneTransitionImage;
    [SerializeField] private float delayBeforeTransition;
    [SerializeField] private float transitionTime;
    private float _transitionTimer;
    
    private bool _allUnlocked;
    
    private enum TransitionState
    {
        None,
        Delay,
        Transition
    }

    private TransitionState _transitionState;
    
    private void Awake()
    {
        LoadFromDisk();
        
        Application.targetFrameRate = frameRate;
        
        sceneTransitionImage.sizeDelta = new Vector2(sceneTransitionCanvas.rect.width, sceneTransitionImage.sizeDelta.y);

        CreateDependencies();
    }
    
    private void Start()
    {
        _dateTimeSinceLastSave = DateTime.Now;
        
        Random.InitState(42);
        
        mainCam.backgroundColor = Color.black;
        
        foreach (Dependency dependencyInUse in _dependencies)
        {
            Dependency dependency = dependencyInUse;
            dependency.GameStart(this);
        }
        
        _transitionState = TransitionState.Delay;
    }
    
    private void Update()
    {
        float dt = Time.deltaTime;
        switch (_transitionState)
        {
            case TransitionState.None:
            {
                if (!firstInputTaken 
                    && (inputSo.leftMouseButton.action.WasPressedThisFrame() || inputSo.rightMouseButton.action.WasPressedThisFrame()))
                {
                    GetDependency<UISystem>().SetHomescreen(false);
                    firstInputTaken = true;
                    return;
                }

                if (!firstInputTaken)
                {
                    GetDependency<UISystem>().GameUpdate(dt);
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    //Give 3 stars to all levels up to 7 - 8
                    saveDataSo.levels = new();
                    for (int sec = 1; sec <= 7; sec++)
                    {
                        for (int lev = 1; lev <= 8; lev++)
                        {
                            saveDataSo.levels.Add(new()
                            {
                                section = sec,
                                level = lev,
                                score = 0,
                                starsScored = 3
                            });
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    //Give 3  stars to all levels up to 8 - 9
                    saveDataSo.levels = new();
                    for (int sec = 1; sec <= 8; sec++)
                    {
                        for (int lev = 1; lev <= 10; lev++)
                        {
                            if (sec == 8 && lev == 10)
                            {
                                continue;
                            }
                            
                            saveDataSo.levels.Add(new()
                            {
                                section = sec,
                                level = lev,
                                score = 0,
                                starsScored = 3
                            });
                        }
                    }
                }
        
                foreach (Dependency dependency in _dependencies)
                {
                    dependency.GameUpdate(dt);
                }
                
                break;
            }
            case TransitionState.Delay:
            {
                _transitionTimer += dt;

                if (_transitionTimer >= delayBeforeTransition)
                {
                    _transitionState = TransitionState.Transition;
                    _transitionTimer = 0;
                }
                break;
            }
            case TransitionState.Transition:
            {
                _transitionTimer += dt;
            
                float t = math.min(1, _transitionTimer / transitionTime);
                float width = math.lerp(sceneTransitionCanvas.rect.width, 0, t);
                sceneTransitionImage.sizeDelta = new Vector2(width, sceneTransitionImage.sizeDelta.y);

                if (t >= 1)
                {
                    sceneTransitionCanvas.gameObject.SetActive(false);
                    
                    _transitionState = TransitionState.None;
                }
                break;
            }
        }
    }

    private void UnlockAll()
    {
        saveDataSo.levels.Clear();
        foreach (SectionData sectionData in levelsSo.sections)
        {
            foreach (Level level in sectionData.levels)
            {
                saveDataSo.levels.Add(new()
                {
                    level = level.level,
                    score = 0,
                    section = level.section,
                    starsScored = 3
                });
            }
        }
    }

    private void CreateDependencies()
    {
        _dependencies.Add(new PoolSystem());
        _dependencies.Add(new UISystem());
        _dependencies.Add(new ValidMovesSystem());
        _dependencies.Add(new PlayerSetTilesSystem());
        _dependencies.Add(new AudioSystem());
        _dependencies.Add(new BoardSystem());
        _dependencies.Add(new WhiteSystem());
        _dependencies.Add(new BlackSystem());
        _dependencies.Add(new EndGameSystem());
        _dependencies.Add(new TurnSystem());
    }

    public void SaveToDisk()
    {
        TimeSpan elapsed = DateTime.Now - _dateTimeSinceLastSave;
        saveDataSo.totalSeconds += elapsed.TotalSeconds;
        _dateTimeSinceLastSave = DateTime.Now;
        
        ES3.Save(settingsSo.saveDataKey, saveDataSo);
    }

    private void LoadFromDisk()
    {
        if (ES3.KeyExists(settingsSo.saveDataKey))
        {
            SaveData_SO diskSaveDataSo = ES3.Load(settingsSo.saveDataKey, saveDataSo);
            saveDataSo.levels = diskSaveDataSo.levels.ToList();
            saveDataSo.audio = diskSaveDataSo.audio;
            saveDataSo.boardVariant = diskSaveDataSo.boardVariant;
            saveDataSo.levelLastLoaded = diskSaveDataSo.levelLastLoaded;
            saveDataSo.isFirstTime = false;
            saveDataSo.windowWidth = diskSaveDataSo.windowWidth;
            saveDataSo.windowHeight = diskSaveDataSo.windowHeight;
            saveDataSo.totalTurns = diskSaveDataSo.totalTurns;
            saveDataSo.totalMoves = diskSaveDataSo.totalMoves;
            saveDataSo.totalCaptures = diskSaveDataSo.totalCaptures;
            saveDataSo.totalSeconds = diskSaveDataSo.totalSeconds;
            saveDataSo.showAbilityText = diskSaveDataSo.showAbilityText;
        }
        else
        {
            saveDataSo.levels = new();
            saveDataSo.audio = true;
            saveDataSo.isFullscreen = false;
            saveDataSo.boardVariant = boardSo.boardVariants[0];
            saveDataSo.sectionLastLoaded = 1;
            saveDataSo.levelLastLoaded = 1;
            saveDataSo.isFirstTime = true;
            saveDataSo.windowWidth = Screen.width;
            saveDataSo.windowHeight = Screen.height;
            saveDataSo.totalTurns = 0;
            saveDataSo.totalMoves = 0;
            saveDataSo.totalCaptures = 0;
            saveDataSo.totalSeconds = 0;
            saveDataSo.showAbilityText = true;
        }
    }

    public void DeleteOnDisk()
    {
        ES3.DeleteKey(settingsSo.saveDataKey);
        
        SceneManager.LoadScene(0);
    }
    
    [Button]
    public void DeleteOnDiskExposed()
    {
        ES3.DeleteKey(settingsSo.saveDataKey);
        Debug.Log("SAVE DATA DELETED");
    }
    
    public GameObject InstantiateGameObject(GameObject creatorGridPrefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(creatorGridPrefab, position, rotation);
    }

    public GameObject InstantiateGameObjectWithParent(GameObject newObject, Transform parent)
    {
        return Instantiate(newObject, parent);
    }
    
    public T GetDependency<T>() where T : Dependency
    {
        foreach (Dependency dependent  in _dependencies)
        {
            if (dependent is T)
            {
                return (T) dependent;
            }
        }

        Debug.LogError("Could not find dependency");
        return null;
    }
    
    public Transform GetFirstObjectWithName(AllTagNames gameObjectName)
    {
        Transform[] objects = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform obj in objects)
        {
            if (obj.TryGetComponent(out TagName objName))
            {
                if (objName.tagName == gameObjectName)
                {
                    return obj;
                }
            }
        }

        Debug.LogError("Could not find object");
        return null;
    }

    public T GetChildComponentByName<T>(GameObject parent, AllTagNames gameObjectName)
    {
        Transform[] objects = parent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform obj in objects)
        {
            if (obj.TryGetComponent(out TagName objName))
            {
                if (objName.tagName == gameObjectName)
                {
                    return obj.GetComponent<T>();
                }
            }
        }

        throw new NullReferenceException("Could not find object");
    }
    
    public List<T> GetChildComponentsByName<T>(GameObject parent, AllTagNames gameObjectName)
    {
        Transform[] objects = parent.GetComponentsInChildren<Transform>(true);

        List<T> objectsWithComponent = new(objects.Length);
        
        foreach (Transform obj in objects)
        {
            if (obj.TryGetComponent(out TagName objName))
            {
                if (objName.tagName == gameObjectName)
                {
                    objectsWithComponent.Add(obj.GetComponent<T>());
                }
            }
        }
        
        return objectsWithComponent;
    }
    
    public Transform GetChildObjectByName(GameObject parent, AllTagNames gameObjectName)
    {
        Transform[] objects = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform obj in objects)
        {
            if (obj.TryGetComponent(out TagName objName))
            {
                if (objName.tagName == gameObjectName)
                {
                    return obj;
                }
            }
        }

        Debug.LogError("Could not find object");
        return null;
    }

    private void OnApplicationQuit()
    {
        saveDataSo.windowWidth = Screen.width;
        saveDataSo.windowHeight = Screen.height;
        
        SaveToDisk();
    }
}

public class PoolSystem : Dependency
{
    private List<GameObject> _piecesPool;

    private List<ScaleTween> _blockedTextEffectPool;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        int capacity = 64;
        _piecesPool = new (capacity);
        _blockedTextEffectPool = new(capacity);
        for (int i = 0; i < _piecesPool.Capacity; i++)
        {
            GameObject pieceGo = creator.InstantiateGameObject(Creator.piecePrefab, new(-100, -100), Quaternion.identity);
            pieceGo.SetActive(false);
            _piecesPool.Add(pieceGo);

            GameObject blockEffectGo = creator.InstantiateGameObject(Creator.blockedTextEffect, new(-100, -100), Creator.blockedTextEffect.transform.rotation);
            blockEffectGo.SetActive(false);
            _blockedTextEffectPool.Add(blockEffectGo.GetComponent<ScaleTween>());
        }
        
    }

    public GameObject GetPieceObjectFromPool(Vector3 position)
    {
        if (_piecesPool.Count == 0)
        {
            GameObject pieceGo =
                Creator.InstantiateGameObject(Creator.piecePrefab, position, Quaternion.identity);
            return pieceGo;
        }

        GameObject piece = _piecesPool[^1];
        piece.transform.position = position;
        piece.SetActive(true);

        _piecesPool.Remove(piece);
        return piece;
    }

    public void ReturnObjectToPool(GameObject go)
    {
        go.SetActive(false);
        go.transform.position = new(-100, -100);
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        
        _piecesPool.Add(go);
    }

    public void ShowBlockedTextEffect(Vector3 position)
    {
        Debug.Log("Showing blocked text effect at "+position);

        position.x += 0.5f;
        position.y += 0.5f;
        
        ScaleTween blockedEffect = _blockedTextEffectPool[0];
        blockedEffect.transform.position = position;
        Fing(blockedEffect);
    }

    private async void Fing(ScaleTween scaleTween)
    {
        scaleTween.gameObject.SetActive(true);

        _blockedTextEffectPool.Remove(scaleTween);
        
        scaleTween.Enlarge();

        await Task.Delay((int)((scaleTween.phaseInTime + scaleTween.phaseOutTime) * 1000));
        
        scaleTween.Shrink();
        
        await Task.Delay((int)(scaleTween.phaseOutTime * 1000));
        
        _blockedTextEffectPool.Add(scaleTween);
        
        scaleTween.gameObject.SetActive(false);
    }
}
