using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : GenericUnit
{

    // Start is called before the first frame update
    void Start()
    {
        base.health = 100;
        base.damageStrength = 1;
        base.reach = 2;
    }

    public override void Attack(GenericUnit enemy)
    {
        Debug.Log("Aaaaattack!");
        enemy.GetHurt(this.damageStrength);        
    }

    public override bool EnemiesInRange(List<GenericUnit> enemyTeam)
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

    public override List<GenericUnit> GetEnemiesInRange(List<GenericUnit> enemyTeam)
    {
        List<GenericUnit> nearby = new List<GenericUnit>();

        foreach(GenericUnit unit in enemyTeam)
        {
            float dist = (unit.transform.position - this.transform.position).magnitude;
            if (dist < this.reach)
            {
                nearby.Add(unit);
            }
        }
        return nearby;
    }

    public override void GetHurt(int value)
    {
        Debug.Log("I'm hurt");
        this.health = Mathf.Max(0, this.health - value);
    }

}