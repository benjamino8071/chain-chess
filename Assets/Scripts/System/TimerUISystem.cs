using TMPro;
using UnityEngine;

public class TimerUISystem : Dependency
{
    private PlayerSystem _playerSystem;
    
    private Transform _timerGUI;
    
    private TextMeshProUGUI _timeText;
    
    private bool _playTimer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        if (_creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }

        _timerGUI = GameObject.FindWithTag("Timer").transform;
        
        _timeText = GameObject.FindWithTag("TimeText").GetComponent<TextMeshProUGUI>();
        
        if (_creator.playerSystemSo.roomNumberSaved == 0)
        {
            _creator.timerSo.currentTime = _creator.timerSo.maxTime;
        }
        
        string timeText = _creator.timerSo.currentTime.ToString("F2")+"s";
        SetTimerText(timeText);
    }

    public override void GameEarlyUpdate(float dt)
    {
        if (_playTimer && _creator.timerSo.currentTime > 0)
        {
            _creator.timerSo.currentTime -= dt;
            string timeText = _creator.timerSo.currentTime.ToString("F2")+"s";
            if (_creator.timerSo.currentTime <= 0)
            {
                timeText = "0.00s";
                _playerSystem.SetState(PlayerSystem.States.Captured);
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
        _timerGUI.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _timerGUI.gameObject.SetActive(false);
    }
}
