using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Board_SO : ScriptableObject
{
    public bool hideMainMenuTrigger;

    public List<BoardVariant> boardVariants;

    public int oneStarConfettiEmissionRate;
    public int twoStarConfettiEmissionRate;
    public int threeStarConfettiEmissionRate;
    
    [Button]
    public void SetSwapColor()
    {
        for (int i = 0; i < boardVariants.Count; i++)
        {
            BoardVariant boardVariant = boardVariants[i];
            
            Color blackColor = boardVariant.colourGradient.colorKeys[0].color;
            Color whiteColor = boardVariant.colourGradient.colorKeys[1].color;
            
            Gradient effectGradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(whiteColor, 0), new GradientColorKey(blackColor, 1) } };

            boardVariant.swappedColourGradient = effectGradient;
            
            boardVariants[i] = boardVariant;
        }
    }
    
}

[Serializable]
public struct BoardVariant
{
    public Sprite board;
    public Gradient colourGradient;
    public Gradient swappedColourGradient;
    public Color edgeColur;
    public Color backgroundColour;
}
