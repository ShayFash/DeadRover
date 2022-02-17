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
        return GetEnemiesInRange(enemyTeam).Count != 0;    
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

    public void ChangeColor()
    {
        SpriteRenderer sp = this.GetComponent<SpriteRenderer>();
        if (sp.color == Color.red)
        {
            sp.color = Color.white;
        }
        else
        {
            sp.color = Color.red;
        }
    }

    void OnMouseDown()
    {
        this.ChangeColor();
    }

}
