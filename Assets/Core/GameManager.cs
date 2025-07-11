using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        // Start the first round
        RoundManager.Instance.StartRound();
    }
}
