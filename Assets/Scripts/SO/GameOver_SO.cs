using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[CreateAssetMenu]
public class GameOver_SO : ScriptableObject
{
    public List<Quote> quotes;

    [SerializeField] private int _previousIdx;
    
    public Quote GetRandomQuote()
    {
        List<Quote> tmp = quotes.ToList();
        tmp.RemoveAt(_previousIdx);
        
        Random rnd = new(DateTime.Now.Millisecond);
        int idx = rnd.Next(tmp.Count);
        _previousIdx = idx;
        return tmp[idx];
    }
}

public enum GameOverReason
{
    Captured,
    NoTurns,
    Locked
}

[Serializable]
public struct Quote
{
    [TextArea(5, 15)]
    public string quote;
    public string name;
}
