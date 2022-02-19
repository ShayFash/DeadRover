using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class GenericUnit : MonoBehaviour
{
    public int reach;
    public int attack;

    public int health;
    protected int maxHealth;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;
    private TextMeshProUGUI healthText;

    protected void Init()
    {
        maxHealth = health;

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log(unitMenuChildren.Length);

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) { 
            return t.gameObject.CompareTag("AttackStatDisplay"); 
        });

        healthText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("HealthStatDisplay");
        });
    }

    public void Attack(GenericUnit enemy)
    {
        Debug.Log("Aaaaattack!");
        enemy.TakeDamage(attack);
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        health = Mathf.Max(0, health - value);
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
            float dist = (unit.transform.position - transform.position).magnitude;
            if (dist < reach)
            {
                nearby.Add(unit);
            }
        }
        return nearby;
    }

    private void OnMouseDown()
    {
        unitMenu.enabled = true;
        attackText.text = attack.ToString() + "Attack";
        healthText.text = health.ToString() + "/" + maxHealth.ToString() + " HP";
    }
}
