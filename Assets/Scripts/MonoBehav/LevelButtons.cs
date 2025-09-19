using System;
using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtons : MonoBehaviour
{
    public List<LevelButton> buttons;
}

[Serializable]
public class LevelButton
{
    public ButtonManager button;
    public Image starOneImage;
    public Image starTwoImage;
    public Image starThreeImage;
}
