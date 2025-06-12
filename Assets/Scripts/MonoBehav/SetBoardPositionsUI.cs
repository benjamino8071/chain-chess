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
            Vector3 worldPosition = tmpText.transform.position;
            string x = $"{worldPosition.x:F1}";
            string y = $"{worldPosition.y:F1}";
            
            tmpText.text = $"({x},{y})";
        }
    }
}
