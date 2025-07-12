using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        DeckManager.Instance.ResetDeck();
        RoundManager.Instance.StartRound();
    }
}
