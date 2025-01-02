using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creator : MonoBehaviour
{
    public GridSystem_SO gridSystemSo;
    public Cinemachine_SO cinemachineSo;
    
    protected LinkedList<Dependency> _dependencies = new();

    private void Awake()
    {
        Application.targetFrameRate = 120;
        
        CreateDependencies();
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

    private void FixedUpdate()
    {
        foreach (Dependency dependency in _dependencies)
        {
            dependency.GameFixedUpdate(Time.fixedDeltaTime);
        }
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

    public void StartACoRoutine(IEnumerator corout)
    {
        StartCoroutine(corout);
    }
    
    public virtual void CreateDependencies()
    {
        
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
}
