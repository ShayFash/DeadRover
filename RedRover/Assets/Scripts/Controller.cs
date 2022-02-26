using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private enum State
    {
        Normal,
        Attacking
    }

    private enum Player
    {
        Living,
        Dead
    }

    private State state = State.Normal;
    private Player activePlayer = Player.Living;   // Living starts
    
    private GenericUnit selectedUnit;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;
    private TextMeshProUGUI healthText;

    private void Start()
    {
        GenericUnit[] units = FindObjectsOfType<GenericUnit>();

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });

        healthText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("HealthStatDisplay");
        });
    }

    public void SelectUnit(GenericUnit unit)
    {
        switch (state)
        {
            case State.Normal:
                if (!unit.CompareTag(activePlayer.ToString())) 
                {
                    return;
                }
                selectedUnit = unit;

                unitMenu.enabled = true;
                attackText.text = unit.Attack.ToString() + " Attack";
                healthText.text = unit.Health.ToString() + "/" + unit.MaxHealth.ToString() + " HP";
                break;

            case State.Attacking:
                if (unit.CompareTag(activePlayer.ToString())) 
                {
                    return;
                }
                if (!selectedUnit.CompareTag(unit.tag) && selectedUnit.UnitInRange(unit))
                {
                    selectedUnit.AttackUnit(unit);

                    EndTurn();
                }
                break;

            default:
                break;
        }
    }

    public void Attack()
    {
        state = State.Attacking;
    }

    public void EndTurn()
    {
        Debug.Log("Turn has ended!");
        state = State.Normal;
        unitMenu.enabled = false;

        ChangeTurns();
    }

    private void ChangeTurns() 
    {
        if (activePlayer == Player.Living) 
        {
            activePlayer = Player.Dead;
        }
        else if (activePlayer == Player.Dead) 
        {
            activePlayer = Player.Living;
        }
    }
}
