using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    public static PlayerHud Instance;
    
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void UpdateHealth(int newHealth)
    {
        healthText.text = newHealth.ToString();
        healthText.color = Color.Lerp(Color.red, Color.green, newHealth / 100f);
    }
}
