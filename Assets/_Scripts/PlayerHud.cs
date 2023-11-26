using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    public static PlayerHud Instance;
    
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject damageDealtPrefab;
    [SerializeField] private GameObject damageReceivedPrefab;
    [SerializeField] private Transform damageVerticalLayoutGroup;

    private int damageTextDuration = 3;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
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

    public void UpdateDamageDealt(GameObject dealtDamageTo, int damageAmount)
    {
        GameObject damageDealtObj = Instantiate(damageDealtPrefab, damageVerticalLayoutGroup);
        TextMeshProUGUI damageDealtUGUI = damageDealtObj.GetComponent<TextMeshProUGUI>();

        if (damageAmount <= 3)
            damageDealtUGUI.text = $"Dealt <color=yellow>{damageAmount}</color> damage to <color=black>{dealtDamageTo.name}</color>";
        else if (damageAmount > 3 && damageAmount <= 8)
            damageDealtUGUI.text = $"Dealt <color=orange>{damageAmount}</color> damage to <color=black>{dealtDamageTo.name}</color>";
        else if (damageAmount > 8)
            damageDealtUGUI.text = $"Dealt <color=red>{damageAmount}</color> damage to <color=black>{dealtDamageTo.name}</color>";
       
        Destroy(damageDealtObj, damageTextDuration); // Destroy the text element after 5 seconds
    }
    
    public void UpdateDamageReceived(GameObject receivedDamageBy, int damageAmount)
    {
        GameObject damageReceivedObj = Instantiate(damageReceivedPrefab, damageVerticalLayoutGroup);
        TextMeshProUGUI damageReceivedUGUI = damageReceivedObj.GetComponent<TextMeshProUGUI>();
        
        if (damageAmount <= 3)
            damageReceivedUGUI.text = $"Taken <color=yellow>{damageAmount}</color> damage by <color=black>{receivedDamageBy.name}</color>";
        else if (damageAmount > 3 && damageAmount <= 8)
            damageReceivedUGUI.text = $"Taken <color=orange>{damageAmount}</color> damage by <color=black>{receivedDamageBy.name}</color>";
        else if (damageAmount > 8)
            damageReceivedUGUI.text = $"Taken <color=red>{damageAmount}</color> damage by <color=black>{receivedDamageBy.name}</color>";
        
        Destroy(damageReceivedObj, damageTextDuration); // Destroy the text element after 5 seconds
    }
}
