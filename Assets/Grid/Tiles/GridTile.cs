using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GridTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer topImage;

    [SerializeField]
    private SpriteRenderer topImageOutline;

    [SerializeField]
    private SpriteRenderer edgeImage;

    [SerializeField]
    private SpriteRenderer edgeImageOutline;

    [SerializeField]
    private const float edgeDarkness = 150f / 255f;

    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    [SerializeField]
    private TileInfo tile;

    private Placeable placedObject;
    private ShakeBehavior shakeBehavior;

    // Properties
    public int X => x;
    public int Y => y;
    public TileInfo Tile => tile;
    public Placeable PlacedObject => placedObject;
    public Vector2Int Position => new Vector2Int(x, y);

    public void Initialize(int xPos, int yPos, TileInfo tile)
    {
        x = xPos;
        y = yPos;

        shakeBehavior = GetComponent<ShakeBehavior>();

        SetTile(tile);
    }

    public void SetTile(TileInfo tile)
    {
        this.tile = tile;
        UpdateVisual();
    }

    public void SetPosition(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
    }

    public void SetPlacedObject(Placeable obj)
    {
        placedObject = obj;
    }

    public void ClearPlacedObject()
    {
        placedObject = null;
    }

    public void Shake()
    {
        if (shakeBehavior != null)
        {
            shakeBehavior.Shake();
        }
    }

    private void UpdateVisual()
    {
        if (topImage == null || edgeImage == null)
            return;

        int rootSortingOrder = Y * -100;
        topImageOutline.sortingOrder = rootSortingOrder + 3;
        topImage.sortingOrder = rootSortingOrder + 2;
        edgeImageOutline.sortingOrder = rootSortingOrder + 1;
        edgeImage.sortingOrder = rootSortingOrder;

        topImage.sprite = tile.TileSprite;
        topImage.color = Color.white;

        edgeImage.sprite = tile.TileSprite;
        edgeImage.color = new Color(edgeDarkness, edgeDarkness, edgeDarkness, 1f);
    }
}
