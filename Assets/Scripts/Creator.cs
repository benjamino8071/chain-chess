using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creator : MonoBehaviour
{
    public GridSystem_SO gridSystemSo;
    
    protected List<Dependency> _dependencies = new();

    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        CreateDependencies();
    }

    private void Update()
    {
        Debug.Log("Update function called!");
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

    public bool NewTryGetDependency<T>(out T dependency) where T : Dependency
    {
        foreach (Dependency dependent  in _dependencies)
        {
            if (dependent is T)
            {
                dependency = (T) dependent;
                return true;
            }
        }

        dependency = default;
        return false;
    }

    public bool TryGetDependency<T>(string className, out T dependency) where T : ElDependency
    {
        foreach (Dependency dependent  in _dependencies)
        {
            if (dependent is T)
            {
                dependency = (T) dependent;
                return true;
            }
        }

        dependency = default;
        return false;
    }

    public void StartACoRoutine(IEnumerator corout)
    {
        StartCoroutine(corout);
    }

    public virtual void CreateDependencies()
    {
        
    }
}
