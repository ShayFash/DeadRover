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

    private State state = State.Normal;
    
    private GenericUnit selectedUnit;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;
    private TextMeshProUGUI healthText;

    private Coroutine cancelCoroutine;

    // Turn-Based System
    private GenericUnit[] P1SelectableUnits;
    private GenericUnit[] P2SelectableUnits;
    private bool P1Action;
    private bool P2Action;
    private bool P1Selecting;
    private bool P2Selecting;
    
    private bool Player1;
    private bool Player2;

    // Turn-System UI messaging
    private string[] messages = {"Selecting", "Performing action", "Waiting"};
    public Text P1Text;
    public Text P2Text;

    private void Start()
    {
        GenericUnit[] units = FindObjectsOfType<GenericUnit>();

        //
        foreach (GenericUnit unit in units) 
        {
            if (unit.tag == "Living") 
            {
                P1SelectableUnits.Add(unit);
            } 
            else if (unit.tag == "Dead") 
            {
                P2SelectableUnits.Add(unit);
            }
        }
        P1Action = true;
        P2Action = false;
        P1Selecting = true;
        P2Selecting = false;
        Player1 = true;
        Player2 = false;

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });

        healthText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("HealthStatDisplay");
        });
    }

    // Update is called once per frame
    void Update() 
    {
        if (P1Selecting) 
        {
            P1Text.text = "Player 1: " + messages[0];
            P2Text.text = "Player 2: " + messages[2];
        }
        else if (P2Selecting) 
        {
            P1Text.text = "Player 1: " + messages[2];
            P2Text.text = "Player 2: " + messages[0]; 
        }
        else if (P1Action)
        {
            P1Text.text = "Player 1: " + messages[1];
            P2Text.text = "Player 2: " + messages[2];
        }
        else if (P2Action)
        {
            P1Text.text = "Player 1: " + messages[2];
            P2Text.text = "Player 2: " + messages[1];
        }
    }

    public void SelectUnit(GenericUnit unit)
    {
        switch (state)
        {
            case State.Normal:
                selectedUnit = unit;

                unitMenu.enabled = true;
                attackText.text = unit.Attack.ToString() + " Attack";
                healthText.text = unit.Health.ToString() + "/" + unit.MaxHealth.ToString() + " HP";
                break;

            case State.Attacking:
                if (!selectedUnit.CompareTag(unit.tag) && selectedUnit.UnitInRange(unit))
                {
                    selectedUnit.AttackUnit(unit);
                    state = State.Normal;
                    unitMenu.enabled = false;
                    StopCoroutine(cancelCoroutine);
                }
                break;

            default:
                break;
        }
    }

    public void Attack()
    {
        state = State.Attacking;
        cancelCoroutine = StartCoroutine(CancelAction());
    }

    IEnumerator CancelAction()
    {
        while (!Input.GetKeyDown(KeyCode.Escape))
        {
            yield return null;
        }
        Debug.Log("Cancelling action");
        state = State.Normal;
    }

    //One unit from one line can be picked to act
    //After a unit acts, a unit from the other line can be picked, etc.

    // Making a character switch teams
    private void SwitchTeams() 
    {
        
    } 

    // Determine selection
    private void CheckSelection() 
    {
        if (Player1) 
        {
            //for(int runs = 0; runs < 400; runs++)
            //{
                //terms[] = runs;
            //}
        }
        else if (Player2) 
        {
            //for(int runs = 0; runs < 400; runs++)
            //{
                //terms[] = runs;
            //}
        }
    }

    // Changing the active player
    private void ChangePlayer() 
    {
        if (Player1) 
        {
            Player1 = false;
            Player2 = true;
        }
        else if (Player2) 
        {
            Player1 = true;
            Player2 = false;
        }
    }

}
