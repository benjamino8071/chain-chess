using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ChainVisuals : MonoBehaviour
{
    public List<SpriteRenderer> chainSprites = new();

    public LineRenderer pieceInUseLineRenderer;
}
