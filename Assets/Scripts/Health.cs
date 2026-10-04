using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [Min(1f)] [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private UnityEvent onDeath;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    private bool dead;

    private void Awake() { CurrentHealth = maxHealth; }

    public void TakeDamage(float amount)
    {
        if (dead || amount <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (CurrentHealth > 0f) return;
        dead = true;
        onDeath.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        if (!dead && amount > 0f) CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }
}