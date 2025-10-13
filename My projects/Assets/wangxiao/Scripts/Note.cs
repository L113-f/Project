using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public bool WasHit { get; private set; } = false;
    public bool WasMissed { get; private set; } = false;

    private float speed;
    private float hitLineY;
    private float tolerance;
    private GameManager manager;
    private Renderer rend;

    public void Initialize(float speed, float hitLineY, float tolerance, GameManager gm)
    {
        this.speed = speed;
        this.hitLineY = hitLineY;
        this.tolerance = tolerance;
        this.manager = gm;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (WasHit || WasMissed) return;

        transform.position += Vector3.down * speed * Time.deltaTime;

        // 过击打线较多后自动判为未命中
        if (transform.position.y < hitLineY - 1f)
        {
            WasMissed = true;
            if (rend != null) rend.material.color = Color.gray;
            Destroy(gameObject);
        }
    }

    public void TryHit()
    {
        float dy = Mathf.Abs(transform.position.y - hitLineY);
        Debug.Log($"尝试命中！dy = {dy}, tolerance = {tolerance}");

        if (dy <= tolerance)
        {
            WasHit = true;
            if (rend != null) rend.material.color = Color.green;
            manager.RegisterHit();
            Destroy(gameObject, 0.1f);
        }
        else
        {
            Debug.Log("未命中：距离过远");
        }
    }
}