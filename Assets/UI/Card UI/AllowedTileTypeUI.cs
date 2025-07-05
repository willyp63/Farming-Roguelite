using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllowedTileTypeUI : MonoBehaviour
{
    [SerializeField]
    private TileType tileType;

    public TileType TileType => tileType;
}
