using TMPro;
using UnityEngine;

public class ElTimerUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElPauseUISystem _pauseUISystem;
    
    private TextMeshProUGUI _timeText;
    
    private bool _playTimer;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        if (Creator.NewTryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (Creator.NewTryGetDependency(out ElPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }
        
        _timeText = GameObject.FindWithTag("Timer").GetComponent<TextMeshProUGUI>();
        
        if (Creator.playerSystemSo.roomNumberSaved == 0 && Creator.playerSystemSo.levelNumberSaved == 0)
        {
            Creator.timerSo.currentTime = Creator.timerSo.maxTime;
        }
        
        string timeText = Creator.timerSo.currentTime.ToString("F2")+"s";
        SetTimerText(timeText);
    }

    public override void GameEarlyUpdate(float dt)
    {
        if (_playTimer && Creator.timerSo.currentTime > 0 && _playerSystem.GetState() != ElPlayerSystem.States.EndGame)
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

    public void StartTimer()
    {
        _playTimer = true;
    }

    public void StopTimer()
    {
        _playTimer = false;
    }
    
    private void Show()
    {
        _timeText.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _timeText.gameObject.SetActive(false);
    }
}
