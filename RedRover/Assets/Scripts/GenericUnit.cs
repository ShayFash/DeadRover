using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericUnit : MonoBehaviour
{
    public int reach;
    public int health;
    public int damageStrength;

    public abstract void Attack(GenericUnit enemy);
    public abstract bool EnemiesInRange(List<GenericUnit> enemy);
    public abstract List<GenericUnit> GetEnemiesInRange(List<GenericUnit> enemyTeam);
    public abstract void GetHurt(int value);

}
