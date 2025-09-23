using System;
using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;

public class SectionButtons : MonoBehaviour
{
    public List<Section> buttons;
}

[Serializable]
public class Section
{
    public int section;
    public ButtonManager button;
}
