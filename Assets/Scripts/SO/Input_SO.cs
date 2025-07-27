using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class Input_SO : ScriptableObject
{
    public InputActionReference leftMouseButton;
    public InputActionReference scrollWheel;
    public float scrollPositionChange;
}
