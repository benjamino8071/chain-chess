using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class SetBoardPositionsUI : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }

    [Button]
    public void SetBoardPositions()
    {
        TMP_Text[] positionTexts = GetComponentsInChildren<TMP_Text>();

        foreach (TMP_Text tmpText in positionTexts)
        {
            Vector3 worldPosition = tmpText.transform.position - new Vector3(0.5f,0.5f,0);
            string x = $"{worldPosition.x:F0}";
            string y = $"{worldPosition.y:F0}";
            
            tmpText.text = $"({x},{y})";
        }
    }
}
