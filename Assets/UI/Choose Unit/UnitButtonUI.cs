using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitButtonUI : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Image image;

    [SerializeField]
    private TextMeshProUGUI text;

    [NonSerialized]
    public UnityEvent<UnitData> onPress = new();

    private UnitData unitData;

    public void Initialize(UnitData unitData)
    {
        this.unitData = unitData;

        text.text = unitData.UnitName;
        image.sprite = unitData.Image;

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onPress?.Invoke(unitData);
    }
}
