using UnityEngine;

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    [SerializeField]
    private FloatingText floatingTextPrefab;

    [SerializeField]
    private Canvas canvas;

    public void SpawnText(string text, Vector3 position, Color color)
    {
        FloatingText instance = Instantiate(floatingTextPrefab, canvas.transform);
        instance.Initialize(text, position, color);
    }
}
