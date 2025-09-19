using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creator : MonoBehaviour
{
    [Header("SOs")]
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
    
    [Header("Prefabs")]
    public GameObject piecePrefab;
    public GameObject validPositionPrefab;
    public GameObject playerSetTilePrefab;
    public GameObject selectedBackgroundPrefab;
    public GameObject levelInfoPrefab;
    public GameObject imagePrefab;

    [Header("Camera")]
    public Camera mainCam;
    
    [Header("Frame Rate")]
    public int frameRate = 120;

    [HideInInspector] public int statsTurns;
    [HideInInspector] public int statsMoves;
    
    private List<Dependency> _dependencies = new();
    
    private void Awake()
    {
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

    private void Update()
    {
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

    public List<Transform> GetChildObjectsByName(GameObject parent, AllTagNames gameObjectName)
    {
        List<Transform> objs = new();
        
        Transform[] objects = parent.GetComponentsInChildren<Transform>(true);

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
