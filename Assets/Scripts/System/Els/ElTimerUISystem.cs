using System.Collections;
using TMPro;
using UnityEngine;

public class ElTimerUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElPauseUISystem _pauseUISystem;
    private ElAudioSystem _audioSystem;
    
    private TextMeshProUGUI _timeText;
    private TextMeshProUGUI _timerBonusText;
    private TextMeshProUGUI _multiplierAmountText;
    
    private bool _playTimer;

    private float _showTimeChangeAmountTimer;
    private float _recentTimeChangeAmount;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        if (Creator.TryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (Creator.TryGetDependency(out ElPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }
        if (Creator.TryGetDependency(out ElAudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        
        _timeText = GameObject.FindWithTag("Timer").GetComponent<TextMeshProUGUI>();
        _timerBonusText = GameObject.FindWithTag("TimerBonus").GetComponent<TextMeshProUGUI>();
        _multiplierAmountText = GameObject.FindWithTag("MultiplierAmount").GetComponent<TextMeshProUGUI>();
        
        string timeText = Creator.timerSo.currentTime.ToString("F2")+"s";
        SetTimerText(timeText);
        
        _multiplierAmountText.text = $"<wave a={0.01f}>Multiplier: 1x</wave>";
        ResetTimerChangedAmount(true);

        if (Creator.timerSo.timePenaltyOnReload > 0)
        {
            RemoveTime(Creator.timerSo.timePenaltyOnReload);
            Creator.timerSo.timePenaltyOnReload = 0;
        }
    }

    public override void GameEarlyUpdate(float dt)
    {
        if (_showTimeChangeAmountTimer > 0)
        {
            _showTimeChangeAmountTimer -= dt;
            if (_showTimeChangeAmountTimer <= 0)
            {
                HideTimerChangeAmount();
            }
        }
        
        if (_playTimer && Creator.timerSo.currentTime > 0 && _playerSystem.GetState() != ElPlayerSystem.States.EndGame && Creator.playerSystemSo.roomNumberSaved != Creator.shopSo.shopRoomNumber && Creator.playerSystemSo.roomNumberSaved < 9)
        {
            Creator.timerSo.currentTime -= dt;
            
            string timeText = Creator.timerSo.currentTime.ToString("F2")+"s";
            if (Creator.timerSo.currentTime <= 0)
            {
                timeText = "0.00s";
                _pauseUISystem.Hide();
                _playerSystem.SetState(ElPlayerSystem.States.TimerExpired);
            }
            SetTimerText(timeText);
        }
    }

    private void SetTimerText(string timeText)
    {
        _timeText.SetText(timeText);
    }

    public void AddTime(float pieceValue)
    {
        float amountToAdd = pieceValue * Creator.timerSo.timerMultiplier;
        Creator.timerSo.currentTime += amountToAdd;
        
        string timeText = Creator.timerSo.currentTime.ToString("F2")+"s";
        SetTimerText(timeText);

        if (_recentTimeChangeAmount >= 0)
        {
            _recentTimeChangeAmount += amountToAdd;
        }
        else
        {
            _recentTimeChangeAmount = amountToAdd;
        }
        
        string amountAddText = $"+{_recentTimeChangeAmount:0.##}s";
        ShowTimerChangeAmount(amountAddText);
        
        Creator.timerSo.timerMultiplier *= Creator.timerSo.timerMultiplierMultiplier;
        
        float waveAmp = Mathf.Clamp(0.01f * Creator.timerSo.timerMultiplier, 0.01f, 0.1f);
        _multiplierAmountText.text = $"<wave a={waveAmp}>Multiplier: {Creator.timerSo.timerMultiplier:0.##}x</wave>";

        float amount = Mathf.Log(Creator.timerSo.timerMultiplier) / Mathf.Log(1.1f) - 1;
        
        float pitch = Mathf.Clamp(0.9f + Mathf.RoundToInt(amount) / 50f, 0.9f, 1f);
        _audioSystem.PlayEnemyCapturedSfx(pitch);
    }

    public void RemoveTime(float amount)
    {
        Creator.timerSo.currentTime -= amount;
        
        string timeText = Creator.timerSo.currentTime.ToString("F2")+"s";
        SetTimerText(timeText);

        if (_recentTimeChangeAmount >= 0)
        {
            _recentTimeChangeAmount = -amount;
        }
        else
        {
            _recentTimeChangeAmount -= amount;
        }
        
        string amountRemoveText = $"{_recentTimeChangeAmount:0.##}s";
        ShowTimerChangeAmount(amountRemoveText, true);

        float pitch = Random.Range(0.9f, 1.1f);
        _audioSystem.PlayTimeLostSfx(pitch);
    }

    public void StartTimer()
    {
        _playTimer = true;
    }

    public void StopTimer()
    {
        _playTimer = false;
    }
    
    private void ShowTimerChangeAmount(string textToShow, bool showInRed = false)
    {
        _timerBonusText.text = textToShow;
        _timerBonusText.color = showInRed ? Color.red : Color.green;
        _showTimeChangeAmountTimer = Creator.timerSo.showTimeChangeAmount;
    }

    public void HideTimerChangeAmount()
    {
        _timerBonusText.text = "";
        _showTimeChangeAmountTimer = 0;
        _recentTimeChangeAmount = 0;
    }

    public void ResetTimerChangedAmount(bool hide)
    {
        if (Creator.timerSo.timerMultiplier != 1)
        {
            Creator.timerSo.timerMultiplier = 1;
            _multiplierAmountText.text = $"<wave a={0.01f}>Multiplier: 1x</wave>";
        }

        if (hide)
        {
            HideTimerChangeAmount();
        }
    }
}
