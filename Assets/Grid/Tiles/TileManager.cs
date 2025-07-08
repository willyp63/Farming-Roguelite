using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : Singleton<TileManager>
{
    [SerializeField]
    private TileData tileData;

    protected override void Awake()
    {
        base.Awake();

        if (tileData == null)
        {
            Debug.LogError("TileData not assigned to TileManager!");
        }
    }

    public static TileInfo GetTileInfo(TileType tileType)
    {
        return Instance.tileData.GetTileInfo(tileType);
    }
}
