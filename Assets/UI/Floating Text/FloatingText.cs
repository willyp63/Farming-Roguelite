using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textComponent;

    [SerializeField]
    private float lifetime = 2f;

    [SerializeField]
    private float riseSpeed = 2f;

    [SerializeField]
    private AnimationCurve fadeCurve;

    private float timer;
    private Vector3 startPosition;

    public void Initialize(string text, Vector3 worldPosition, Color color)
    {
        textComponent.text = text;
        textComponent.color = color;

        transform.position = worldPosition;
        startPosition = worldPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        transform.position = startPosition + Vector3.up * (riseSpeed * timer);

        // Fade out
        float alpha = fadeCurve.Evaluate(timer / lifetime);
        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;

        // Destroy when done
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
