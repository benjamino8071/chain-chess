using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElLivesUISystem : ElDependency
{
    private LinkedList<Image> _lives = new();
    private LinkedListNode<Image> _currentLife;

    private Image _invincibleLifeImage;
    private Animator _invincibleLifeAnimator;
    private static readonly int IsSuper = Animator.StringToHash("is_super");

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        Transform livesParent = elCreator.GetFirstObjectWithName(AllTagNames.Lives);

        LivesOrder livesOrder = livesParent.GetComponent<LivesOrder>();

        _invincibleLifeImage = livesOrder.invincibleLife;
        _invincibleLifeAnimator = livesOrder.invincibleLife.GetComponent<Animator>();

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
        
        ShowInvincibleLife(true); //Player will always start level on super invincible life
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

    public bool HasLife()
    {
        return _currentLife is not null;
    }

    public void ShowInvincibleLife(bool isSuperInvincible)
    {
        _invincibleLifeImage.enabled = true;
        _invincibleLifeAnimator.SetBool(IsSuper, isSuperInvincible);
    }

    public void HideInvincibleLife()
    {
        _invincibleLifeImage.enabled = false;
        _invincibleLifeAnimator.SetBool(IsSuper, false);
    }
}
