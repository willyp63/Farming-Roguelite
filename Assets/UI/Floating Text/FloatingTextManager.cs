using UnityEngine;

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    [SerializeField]
    private FloatingText floatingTextPrefab;

    [SerializeField]
    private Canvas canvas;

    private static readonly Color pointsColor = new Color(0.01568628f, 0.5450981f, 0.9411765f, 1f);
    private static readonly Color multiColor = new Color(0.9607843f, 0.282353f, 0.2509804f, 1f);

    public void SpawnText(string text, Vector3 position, Color color)
    {
        FloatingText instance = Instantiate(floatingTextPrefab, canvas.transform);
        instance.Initialize(text, position, color);
    }

    public void SpawnPointsText(int points, int multi, Vector3 position)
    {
        string pointsText = points > 0 ? $"+{points}" : points.ToString();
        string multiText = multi > 0 ? $"+{multi}" : multi.ToString();

        if (points != 0 && multi != 0)
            SpawnText(
                $"<color=#{ColorUtility.ToHtmlStringRGB(pointsColor)}>{pointsText}</color>\n<color=#{ColorUtility.ToHtmlStringRGB(multiColor)}>{multiText}</color>",
                position,
                Color.white
            );
        else if (points != 0)
            SpawnText(pointsText, position, pointsColor);
        else if (multi != 0)
            SpawnText(multiText, position, multiColor);
    }
}
