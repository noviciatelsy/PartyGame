using UnityEngine;

public class SpringButton : MonoBehaviour
{
    public float stiffness = 350f;
    public float damping = 18f;

    public float pressImpulse = -6f;   // 按下冲量
    public float maxScale = 1.2f;      // 最大拉伸

    float value = 1f;
    float velocity = 0f;
    
    void Update()
    {
        float dt = Time.deltaTime;

        float force = stiffness * (1f - value) - damping * velocity;

        velocity += force * dt;
        value += velocity * dt;

        value = Mathf.Clamp(value, 0.7f, maxScale);

        transform.localScale = Vector3.one * value;
    }

    public void Press()
    {
        velocity += pressImpulse;
    }
}