using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElLivesUISystem : ElDependency
{
    private LinkedList<Image> _lives = new();
    private LinkedListNode<Image> _currentLife;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        Transform livesParent = elCreator.GetFirstObjectWithName(AllTagNames.Lives);

        LivesOrder livesOrder = livesParent.GetComponent<LivesOrder>();

        foreach (Image life in livesOrder.lives)
        {
            _lives.AddLast(life);
        }

        _currentLife = _lives.First;

        for (int i = 0; i < Creator.livesSo.maxLives - Creator.livesSo.livesRemaining; i++)
        {
            if(_currentLife is null)
                return;
            
            Color fadedColor = new Color(_currentLife.Value.color.r, _currentLife.Value.color.g, _currentLife.Value.color.b,
                0.1f);
            _currentLife.Value.color = fadedColor;

            _currentLife = _currentLife.Next;
        }
    }

    public void LoseLife()
    {
        if(_currentLife is null)
            return;
        Creator.livesSo.livesRemaining--;
        Color fadedColor = new Color(_currentLife.Value.color.r, _currentLife.Value.color.g, _currentLife.Value.color.b,
            0.1f);
        _currentLife.Value.color = fadedColor;

        _currentLife = _currentLife.Next;
    }

    public bool IsDead()
    {
        return _currentLife is null;
    }
}
