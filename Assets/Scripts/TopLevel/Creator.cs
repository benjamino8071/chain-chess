using System;
using System.Collections.Generic;
using System.Linq;
using Michsky.MUIP;
using Sirenix.OdinInspector;
using TMPro;
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
    public LevelSelect_SO levelSelectSo;
    public MiscUI_SO miscUiSo;
    public Rulebook_SO rulebookSo;

    [Title("Prefabs")]
    public GameObject piecePrefab;
    public GameObject validPositionPrefab;
    public GameObject playerSetTilePrefab;
    public GameObject selectedBackgroundPrefab;
    public GameObject levelInfoPrefab;
    public GameObject imagePrefab;
    public GameObject rulebookHighlightImagePrefab;
    public GameObject colourVariantButtonPrefab;

    [Title("Camera")]
    public Camera mainCam;
    
    [Title("Frame Rate")]
    public int frameRate = 120;

    [HideInInspector] public int statsTurns;
    [HideInInspector] public int statsMoves;
    
    private List<Dependency> _dependencies = new();
    
    private const string _saveDataKey = "saveDataSo";
    
    private void Awake()
    {
        LoadFromDisk();
        
        Application.targetFrameRate = frameRate;
        
        CreateDependencies();
    }
    
    private void Start()
    {
        Random.InitState(42);
        
        mainCam.backgroundColor = Color.black;
        
        foreach (Dependency dependencyInUse in _dependencies)
        {
            Dependency dependency = dependencyInUse;
            dependency.GameStart(this);
        }
    }

    [HideInInspector] private bool _allUnlocked;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !_allUnlocked)
        {
            UnlockAll();
            _allUnlocked = true;
            Debug.Log("ALL STARS UNLOCKED!");
        }
        
        foreach (Dependency dependency in _dependencies)
        {
            dependency.GameEarlyUpdate(Time.deltaTime);
        }
        
        foreach (Dependency dependency in _dependencies)
        {
            dependency.GameUpdate(Time.deltaTime);
        }
        
        foreach (Dependency dependency in _dependencies)
        {
            dependency.GameLateUpdate(Time.deltaTime);
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
        ES3.Save(_saveDataKey, saveDataSo);
        
        Debug.Log("LATEST SAVE DATA SAVED TO DISK");
    }

    private void LoadFromDisk()
    {
        if (ES3.KeyExists(_saveDataKey))
        {
            SaveData_SO diskSaveDataSo = ES3.Load(_saveDataKey, saveDataSo);
            saveDataSo.levels = diskSaveDataSo.levels.ToList();
            saveDataSo.audio = diskSaveDataSo.audio;
            saveDataSo.boardVariant = diskSaveDataSo.boardVariant;
            saveDataSo.levelLastLoaded = diskSaveDataSo.levelLastLoaded;
            saveDataSo.isFirstTime = false;
            
            Debug.Log("LATEST SAVE DATA LOADED FROM DISK");
        }
        else
        {
            saveDataSo.levels = new();
            saveDataSo.audio = true;
            saveDataSo.boardVariant = boardSo.boardVariants[0];
            saveDataSo.levelLastLoaded = levelsSo.GetLevel(1, 1);
            saveDataSo.isFirstTime = true;
            
            Debug.Log("SAVE DATA INITIALISED");
        }
    }

    public void DeleteOnDisk()
    {
        ES3.DeleteKey(_saveDataKey);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        Debug.Log("SAVE DATA DELETED");
    }
    
    [Button]
    public void DeleteOnDiskExposed()
    {
        ES3.DeleteKey(_saveDataKey);
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
        return default;
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

    public List<Transform> GetObjectsByName(AllTagNames gameObjectName)
    {
        List<Transform> objs = new();
        
        Transform[] objects = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform obj in objects)
        {
            if (obj.TryGetComponent(out TagName objName))
            {
                if (objName.tagName == gameObjectName)
                {
                    objs.Add(obj);
                }
            }
        }
        
        return objs;
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
        
        Debug.LogError("Could not find object");
        return default;
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

    private void OnDestroy()
    {
        foreach (Dependency dependency in _dependencies)
        {
            dependency.Destroy();
        }
    }
}
