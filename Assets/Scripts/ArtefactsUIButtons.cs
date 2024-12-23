using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactsUIButtons : MonoBehaviour
{
    public Button buttonOne;
    public Button buttonTwo;
    public Button buttonThree;

    [Header("Sell Buttons")]
    public Button sellButtonOne;
    public Button sellButtonTwo;
    public Button sellButtonThree;

    public List<(Button, Button)> GetButtonsAsList()
    {
        List<(Button, Button)> buttons = new();
        
        buttons.Add((buttonOne, sellButtonOne));
        buttons.Add((buttonTwo, sellButtonTwo));
        buttons.Add((buttonThree, sellButtonThree));

        return buttons;
    }
}
