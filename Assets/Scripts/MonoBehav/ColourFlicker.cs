using System.Collections;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class ColourFlicker : MonoBehaviour
{
    public MMF_Player mmfPlayer;
    public Color newColor;
    public float time;
    public float scale;
    
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    [Button]
    public void ChangeColour()
    {
        mmfPlayer.PlayFeedbacks();
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FlickerColour());
    }
    
    private IEnumerator FlickerColour()
    {
        Color originalColor = _spriteRenderer.color;
        
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            float t = elapsedTime / time;
            
            Color color = Color.Lerp(originalColor, newColor, t);
            _spriteRenderer.color = color;

            transform.localScale = math.lerp(new float3(1), new(scale), t);
            
            // wait one frame
            yield return null; 
            
            elapsedTime += Time.deltaTime; 
        }

        elapsedTime = 0;
        
        while (elapsedTime < time)
        {
            float t = elapsedTime / time;
            
            Color color = Color.Lerp(newColor, originalColor, t);
            _spriteRenderer.color = color;
            
            transform.localScale = math.lerp(new float3(scale), new(1), t);
            
            // wait one frame
            yield return null; 
            
            elapsedTime += Time.deltaTime; 
        }
        
        _spriteRenderer.color = originalColor;
    }
}
