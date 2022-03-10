using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementBar : MonoBehaviour
{
    public Slider slider;
   
    public void SetMaxMovement(int movement)
    {
        slider.maxValue = movement;
        slider.value = movement;
    }
   
    public void SetMovementBar(int movement)
    {
        slider.value = movement;
    }
}
