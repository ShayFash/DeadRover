using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    private TextMeshProUGUI NotificationText;
    
    void Start()
    {
        NotificationText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowNotification(string nType, string nValue) 
    {
        if (nType.Equals("n") ) 
        {
            if (NotificationText.enabled) StartCoroutine(DelayNotification(nValue, new Color32(255, 0, 255, 255)));
            else StartCoroutine(ShowNotificationText(nValue, new Color32(255, 0, 255, 255)));
        }
        else if (nType.Equals("-")) 
        {
            if (NotificationText.enabled) StartCoroutine(DelayNotification(nType + nValue, new Color32(255, 0, 255, 255)));
            else StartCoroutine(ShowNotificationText(nType + nValue, new Color32(255, 0, 0, 255)));
        }
        else if (nType.Equals("+")) 
        {
            if (NotificationText.enabled) StartCoroutine(DelayNotification(nType + nValue, new Color32(255, 0, 255, 255)));
            else StartCoroutine(ShowNotificationText(nType + nValue, new Color32(0, 150, 0, 255)));
        }
    }
    
    private IEnumerator ShowNotificationText(string value1, Color color1)
    {
        NotificationText.text = value1;
        NotificationText.color = color1;
        NotificationText.enabled = true;
        yield return new WaitForSeconds(2f);
        NotificationText.enabled = false;
    }

    private IEnumerator DelayNotification(string value2, Color color2) 
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(ShowNotificationText(value2, color2));
    }
}
