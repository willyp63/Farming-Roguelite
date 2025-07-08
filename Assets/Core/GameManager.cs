using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        // Initialize grid
        GridManager.Instance.InitializeGrid();
        GridUIManager.Instance.InitializeGridUI();

        // Start the first round
        RoundManager.Instance.StartRound();
    }
}
