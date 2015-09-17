using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    public Slider m_HealthBar;      // UI healthbar
    private float m_MaxHealth = 100; // Max health
    public float testHealth;
    private float m_CurrentHealth;   // The entity's current health

    public float m_Health
    {
        get
        {
            return m_CurrentHealth;                 // Returns the entity's current health
        }
        set
        {
            m_CurrentHealth = value;                       // Sets current health to new value
            m_HealthBar.value = m_CurrentHealth;    // Adjust health bar to new current health
        }
    }

    void Heal(float a_heal)
    {
        m_Health += a_heal;             // Increases current health by amount healed

        if (m_Health > m_MaxHealth)     // If the current health is greater than the max health
        {                               //
            m_Health = m_MaxHealth;     // Set the current health to the max health
        }
    }

    void Start()
    {
        m_Health = m_MaxHealth;
    }

    void Update()
    {
        m_Health = testHealth;
    }
}