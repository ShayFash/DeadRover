using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericUnit : MonoBehaviour
{
    public int reach;
    public int health;
    public int damageStrength;

    public abstract void Attack(GenericUnit enemy);
    public abstract bool EnemiesInRange(GenericUnit enemy);
    public abstract GenericUnit GetEnemiesInRange();
    public abstract void GetHurt(int value);

}
