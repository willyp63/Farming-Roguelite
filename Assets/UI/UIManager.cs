using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private TextMeshProUGUI requiredScoreText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private EnergyBarUI energyBarPrefab;

    [SerializeField]
    private Transform energyBarContainer;

    [SerializeField]
    private GameObject chooseUnitPanel;

    [SerializeField]
    private List<UnitButtonUI> chooseUnitButtons;

    [SerializeField]
    private Button skipChoosingUnitButton;

    [System.NonSerialized]
    public UnityEvent<UnitData> onUnitSelected = new();

    [System.NonSerialized]
    public UnityEvent onSkipUnitSelection = new();

    private List<EnergyBarUI> energyBars = new List<EnergyBarUI>();

    public void Start()
    {
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);
        EnergyManager.Instance.OnEnergyChanged.AddListener(OnEnergyChanged);

        CreateEnergyBars();
        OnScoreChanged();
    }

    public void ShowChooseUnitUI()
    {
        // Get 3 random units from the UnitManager
        List<UnitData> randomUnits = UnitManager.Instance.GetRandomUnits(3);

        // Initialize the UnitButtons and show the panel
        for (int i = 0; i < chooseUnitButtons.Count && i < randomUnits.Count; i++)
        {
            chooseUnitButtons[i].gameObject.SetActive(true);
            chooseUnitButtons[i].Initialize(randomUnits[i]);
            chooseUnitButtons[i].onPress.AddListener(OnUnitButtonPressed);
        }

        // Hide any unused buttons
        for (int i = randomUnits.Count; i < chooseUnitButtons.Count; i++)
        {
            chooseUnitButtons[i].gameObject.SetActive(false);
        }

        // Setup skip button
        skipChoosingUnitButton.onClick.RemoveAllListeners();
        skipChoosingUnitButton.onClick.AddListener(OnSkipButtonPressed);

        // Show the panel
        chooseUnitPanel.SetActive(true);
    }

    public void HideChooseUnitUI()
    {
        chooseUnitPanel.SetActive(false);

        // Remove listeners to prevent memory leaks
        foreach (var button in chooseUnitButtons)
        {
            if (button != null)
            {
                button.onPress.RemoveAllListeners();
            }
        }
    }

    private void OnUnitButtonPressed(UnitData unitData)
    {
        HideChooseUnitUI();
        onUnitSelected?.Invoke(unitData);
    }

    private void OnSkipButtonPressed()
    {
        HideChooseUnitUI();
        onSkipUnitSelection?.Invoke();
    }

    private void OnScoreChanged()
    {
        requiredScoreText.text =
            $"<color=#{ColorUtility.ToHtmlStringRGB(FloatingTextManager.pointsColor)}>{RoundManager.Instance.RequiredScore}</color>";

        scoreText.text =
            $"<color=#{ColorUtility.ToHtmlStringRGB(FloatingTextManager.pointsColor)}>{RoundManager.Instance.Score}</color>";

        if (RoundManager.Instance.Score > 0)
        {
            scoreText.GetComponent<ShakeBehavior>().Shake();
        }
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
            SeasonType.Dead,
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
                energyBar.Shake();
                break;
            }
        }
    }
}
