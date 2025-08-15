using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[CreateAssetMenu]
public class GameOver_SO : ScriptableObject
{
    public List<Quote> quotes;

    public Quote GetRandomQuote()
    {
        Random rnd = new(DateTime.Now.Millisecond);
        int idx = rnd.Next(quotes.Count);
        return quotes[idx];
    }
}

[Serializable]
public struct Quote
{
    [TextArea(5, 15)]
    public string quote;
    public string name;
}
