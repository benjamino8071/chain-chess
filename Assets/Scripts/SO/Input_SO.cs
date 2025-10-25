using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class Input_SO : ScriptableObject
{
    public InputActionReference leftMouseButton;
    public InputActionReference rightMouseButton;
    public InputActionReference scrollWheel;
    public InputActionReference exitFullscreen;
    public InputActionReference toggleFullscreen;
    
    public float scrollPositionChange;
}
