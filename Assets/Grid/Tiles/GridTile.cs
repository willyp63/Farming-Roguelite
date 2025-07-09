using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer topImage;

    [SerializeField]
    private SpriteRenderer edgeImage;

    [SerializeField]
    private const float edgeDarkness = 150f / 255f;

    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    [SerializeField]
    private TileInfo tile;

    private Placeable placedObject;

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

    public void Shake(float duration = 0.3f, float intensity = 0.1f)
    {
        StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float xOffset = Mathf.Sin(elapsed * 50f) * intensity;
            float yOffset = Mathf.Cos(elapsed * 30f) * intensity * 0.5f;

            transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void UpdateVisual()
    {
        if (topImage == null || edgeImage == null)
            return;

        topImage.sortingOrder = Y * -1;
        edgeImage.sortingOrder = Y * -1 - 1;

        topImage.sprite = tile.TileSprite;
        topImage.color = Color.white;

        edgeImage.sprite = tile.TileSprite;
        edgeImage.color = new Color(edgeDarkness, edgeDarkness, edgeDarkness, 1f);
    }
}
