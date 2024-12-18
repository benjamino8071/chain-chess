using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class MainMenu_SO : ScriptableObject
{
    [FormerlySerializedAs("isPuzzleCanvasShowing")] public bool isOtherMainMenuCanvasShowing;
}
