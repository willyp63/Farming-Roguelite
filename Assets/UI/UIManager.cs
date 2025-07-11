using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Text Elements")]
    [SerializeField]
    private TextMeshProUGUI requiredScoreText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI moneyText;

    [SerializeField]
    private EnergyBarUI energyBarPrefab;

    [SerializeField]
    private Transform energyBarContainer;

    private List<EnergyBarUI> energyBars = new List<EnergyBarUI>();

    public void Start()
    {
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);
        EnergyManager.Instance.OnEnergyChanged.AddListener(OnEnergyChanged);

        CreateEnergyBars();
        OnScoreChanged();
    }

    private void OnScoreChanged()
    {
        requiredScoreText.text = RoundManager.Instance.RequiredScore.ToString();
        scoreText.text = RoundManager.Instance.Score.ToString();
    }

    private void CreateEnergyBars()
    {
        if (energyBarPrefab == null)
        {
            Debug.LogError("Energy bar prefab not assigned to UIManager!");
            return;
        }

        if (energyBarContainer == null)
        {
            Debug.LogError("Energy bar container not assigned to UIManager!");
            return;
        }

        // Clear any existing energy bars
        foreach (var energyBar in energyBars)
        {
            if (energyBar != null)
            {
                Destroy(energyBar.gameObject);
            }
        }
        energyBars.Clear();

        // Define the season types we want to display (excluding None and Wild)
        SeasonType[] seasonTypes =
        {
            SeasonType.Spring,
            SeasonType.Summer,
            SeasonType.Autumn,
            SeasonType.Winter,
            SeasonType.Death,
        };

        // Create an energy bar for each season type
        foreach (var seasonType in seasonTypes)
        {
            EnergyBarUI newEnergyBar = Instantiate(energyBarPrefab, energyBarContainer);
            energyBars.Add(newEnergyBar);

            // Initialize the energy bar
            newEnergyBar.Initialize(seasonType, EnergyManager.Instance.MaxEnergy);

            // Set initial energy value
            int currentEnergy = EnergyManager.Instance.GetEnergy(seasonType);
            newEnergyBar.SetEnergy(currentEnergy);

            Debug.Log($"Created energy bar for {seasonType} with {currentEnergy} energy");
        }
    }

    private void OnEnergyChanged(SeasonType seasonType, int newEnergy)
    {
        // Find the energy bar for this season type and update it
        foreach (var energyBar in energyBars)
        {
            if (energyBar != null && energyBar.SeasonType == seasonType)
            {
                energyBar.SetEnergy(newEnergy);
                Debug.Log($"Updated energy bar for {seasonType} to {newEnergy}");
                break;
            }
        }
    }
}
