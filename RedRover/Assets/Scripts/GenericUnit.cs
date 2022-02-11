using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericUnit : MonoBehaviour
{
    public int reach;
    public int health;
    public int attack;

    public void Attack(GenericUnit enemy)
    {
        Debug.Log("Aaaaattack!");
        enemy.TakeDamage(this.attack);
    }

    public bool EnemiesInRange(List<GenericUnit> enemyTeam)
    {
        foreach (GenericUnit unit in enemyTeam)
        {
            float dist = (unit.transform.position - this.transform.position).sqrMagnitude;
            if (dist < Mathf.Pow(this.reach, 2))
            {
                return true;
            }
        }
        return false;
    }
    public List<GenericUnit> GetEnemiesInRange(List<GenericUnit> enemyTeam)
    {
        List<GenericUnit> nearby = new List<GenericUnit>();

        foreach (GenericUnit unit in enemyTeam)
        {
            float dist = (unit.transform.position - this.transform.position).magnitude;
            if (dist < this.reach)
            {
                nearby.Add(unit);
            }
        }
        return nearby;
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        this.health = Mathf.Max(0, this.health - value);
    }

}
