using System.Collections.Generic;
using UnityEngine;

public abstract class GenericUnit : MonoBehaviour
{
    public int Reach;
    public int Attack;
    public int Health;
    [HideInInspector]
    public int MaxHealth;

    protected Controller Controller;

    protected void Init()
    {
        MaxHealth = Health;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();
    }


    public void AttackUnit(GenericUnit unit)
    {
        Debug.Log("Aaaaattack!");
        unit.TakeDamage(Attack);
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        Health = Mathf.Max(0, Health - value);
    }

    public bool UnitInRange(GenericUnit unit)
    {
        // TODO: Use tiles
        return (unit.transform.position - transform.position).magnitude < Reach;
    }

    public List<GenericUnit> UnitsInRange(List<GenericUnit> units)
    {
        List<GenericUnit> nearby = new List<GenericUnit>();

        foreach (GenericUnit unit in units)
        {
            // TODO: use tiles
            float dist = (unit.transform.position - transform.position).magnitude;
            if (dist < Reach)
            {
                nearby.Add(unit);
            }
        }
        return nearby;
    }

    private void OnMouseDown()
    {
        Controller.SelectUnit(this);
    }
}
