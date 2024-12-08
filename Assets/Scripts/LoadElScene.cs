using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadElScene : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync("EndlessScene");
    }
}
