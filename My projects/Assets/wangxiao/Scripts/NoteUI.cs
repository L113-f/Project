using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    public bool WasHit { get; private set; } = false;
    public bool WasMissed { get; private set; } = false;

    private float speed;
    private float hitLineY;
    private float tolerance;
    private GameManager manager;
    private RectTransform rect;

    public void Initialize(float speed, float hitLineY, float tolerance, GameManager gm)
    {
        this.speed = speed;
        this.hitLineY = hitLineY;
        this.tolerance = tolerance;
        this.manager = gm;
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (WasHit || WasMissed) return;

        rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        if (rect.anchoredPosition.y < hitLineY - 100f)
        {
            WasMissed = true;
            manager.RemoveNote(this);
            Destroy(gameObject);
        }
    }

    public void TryHit()
    {
        float dy = Mathf.Abs(rect.anchoredPosition.y - hitLineY);
        Debug.Log($"≥¢ ‘√¸÷–£°dy = {dy}, tolerance = {tolerance}");

        if (dy <= tolerance)
        {
            WasHit = true;
            GetComponent<Image>().color = Color.green;
            manager.RegisterHit();
            manager.RemoveNote(this);
            Destroy(gameObject, 0.1f);
        }
        else
        {
            Debug.Log("Œ¥√¸÷–£∫æ‡¿Îπ˝‘∂");
        }
    }
}
