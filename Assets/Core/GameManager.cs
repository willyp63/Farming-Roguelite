using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private bool isShowingDeck = false;

    private int round = 0;

    private void Start()
    {
        DeckManager.Instance.Initialize();

        isShowingDeck = false;
        DeckBoardManager.Instance.HideDeck();
        BoardManager.Instance.HideBoard();

        RoundManager.Instance.OnRoundEnd.AddListener(OnRoundEnd);

        // Listen for unit selection events
        UIManager.Instance.onUnitSelected.AddListener(OnUnitSelected);
        UIManager.Instance.onSkipUnitSelection.AddListener(OnSkipUnitSelection);

        // Listen for tile selection in deck
        DeckBoardManager.Instance.onTileSelected.AddListener(OnDeckTileSelected);

        StartNextRound();
    }

    private void OnRoundEnd()
    {
        round++;

        BoardManager.Instance.HideBoard();
        DeckBoardManager.Instance.HideDeck();

        // Show the choose unit UI
        UIManager.Instance.ShowChooseUnitUI();
    }

    private void OnUnitSelected(UnitData unitData)
    {
        // Show the deck and enter selection mode
        DeckBoardManager.Instance.ShowDeck();
        DeckBoardManager.Instance.EnterSelectionMode(unitData);
    }

    private void OnSkipUnitSelection()
    {
        // Skip unit selection, start next round immediately
        StartNextRound();
    }

    private void OnDeckTileSelected(BoardTile tile)
    {
        // Unit has been placed on a tile, start the next round
        StartNextRound();
    }

    private void StartNextRound()
    {
        // Hide the deck
        DeckBoardManager.Instance.HideDeck();

        // Start the next round
        int requiredScore = 300 + round * 100;
        RoundManager.Instance.StartRound(requiredScore);
        BoardManager.Instance.ShowBoard();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isShowingDeck = !isShowingDeck;
            if (isShowingDeck)
            {
                DeckBoardManager.Instance.ShowDeck();
                BoardManager.Instance.HideBoard();
            }
            else
            {
                DeckBoardManager.Instance.HideDeck();
                BoardManager.Instance.ShowBoard();
            }
        }
    }
}
