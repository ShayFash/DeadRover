using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    public int health = 100;

    public void Damage(int value)
    {
        health = Mathf.Max(0, health - value);
    }
}
