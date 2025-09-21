using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Rulebook_SO : ScriptableObject
{
    public List<Page> pages;
}

[Serializable]
public struct Page
{
    public Sprite sprite;
    public Material material;
    public Color color;
    
    [TextArea(5, 15)]
    public string description;
}
