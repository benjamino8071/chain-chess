using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ElLetterChoiceController : ElController
{
    private List<char> _letters = new();
    private int _letterChosenIndex;
    
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        for (char c = 'A'; c <= 'Z'; c++)
        {
            _letters.Add(c);
        }
    }

    public void Initialise(TextMeshProUGUI choiceText, Button upButton, Button downButton)
    {
        upButton.onClick.AddListener(() =>
        {
            if (_letterChosenIndex == 0)
            {
                _letterChosenIndex = _letters.Count - 1;
            }
            else
            {
                _letterChosenIndex--;
            }

            choiceText.text = _letters[_letterChosenIndex].ToString();
        });
        
        downButton.onClick.AddListener(() =>
        {
            if (_letterChosenIndex == _letters.Count - 1)
            {
                _letterChosenIndex = 0;
            }
            else
            {
                _letterChosenIndex++;
            }
            
            choiceText.text = _letters[_letterChosenIndex].ToString();
        });

        choiceText.text = _letters[_letterChosenIndex].ToString();
    }

    public char LetterChosen()
    {
        return _letters[_letterChosenIndex];
    }
}
