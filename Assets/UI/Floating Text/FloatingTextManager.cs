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

    public void SpawnPointsText(int points, Vector3 position)
    {
        if (points > 0)
            SpawnText($"+{points}", position, new Color(0.01568628f, 0.5450981f, 0.9411765f, 1f));
    }

    public void SpawnMultiText(int multi, Vector3 position)
    {
        if (multi > 0)
            SpawnText($"+{multi}", position, new Color(0.9607843f, 0.282353f, 0.2509804f, 1f));
    }
}
