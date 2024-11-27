using System.Collections.Generic;
using UnityEngine;

public class TileController : Controller
{
    private Creator _creator;
    
    private Transform _tileInstance;

    private SpriteRenderer _tileHighlight;
    
    public void SetTile(Transform tile, Creator creator)
    {
        _creator = creator;
        
        _tileInstance = tile;
        
        List<SpriteRenderer> tileChildren = new List<SpriteRenderer>(_tileInstance.GetComponentsInChildren<SpriteRenderer>(true));
        foreach (SpriteRenderer childSprRend in tileChildren)
        {
            if (childSprRend.tag.Contains("TileHighlight"))
            {
                _tileHighlight = childSprRend;
                ToggleHighlight(false);
                break;
            }
        }
    }
    
    public void SetTile(Transform tile, bool isOffset, Creator creator)
    {
        _creator = creator;
        
        _tileInstance = tile;

        SpriteRenderer spriteRenderer = _tileInstance.GetComponent<SpriteRenderer>();

        List<SpriteRenderer> tileChildren = new List<SpriteRenderer>(_tileInstance.GetComponentsInChildren<SpriteRenderer>(true));
        foreach (SpriteRenderer childSprRend in tileChildren)
        {
            if (childSprRend.tag.Contains("TileHighlight"))
            {
                _tileHighlight = childSprRend;
                ToggleHighlight(false);
                break;
            }
        }

        spriteRenderer.color = isOffset ? _creator.gridSystemSo.offsetColor : _creator.gridSystemSo.baseColor;
    }

    public void ToggleHighlight(bool show)
    {
        _tileHighlight.enabled = show;
    }

    public Vector3 GetPosition()
    {
        return _tileInstance.position;
    }
}
