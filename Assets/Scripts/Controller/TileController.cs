using System.Collections.Generic;
using UnityEngine;

public class TileController : Controller
{
    private ElCreator _elCreator;
    
    private Transform _tileInstance;

    private SpriteRenderer _tileHighlight;
    
    public void SetTile(Transform tile, ElCreator elCreator)
    {
        _elCreator = elCreator;
        
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
    
    public void SetTile(Transform tile, bool isOffset, ElCreator elCreator)
    {
        _elCreator = elCreator;
        
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

        spriteRenderer.color = isOffset ? _elCreator.gridSystemSo.offsetColor : _elCreator.gridSystemSo.baseColor;
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
