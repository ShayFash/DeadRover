using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    private enum State
    {
        SelectingUnit,
        Attacking,
        Moving
    }

    private enum Player
    {
        Living,
        Dead
    }

    private State state = State.SelectingUnit;
    private Player activePlayer = Player.Living;   // Living starts
    
    private GenericUnit selectedUnit;
    private GenericUnit[] units;

    private Tilemap tilemap;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;
    private Button[] actionButtons;

    private AI ai;

    private readonly int SELECTIONTIMERMAX = 3;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == State.Moving)
        {
            Vector3Int mousePosOnGrid = GetClickedGridPosition();
            if (MoveSelectedUnit(mousePosOnGrid))
            {
                // TODO: prevent them from moving again
                state = State.SelectingUnit;
            }
        }
    }

    private void Awake()
    {
        tilemap = FindObjectOfType<Tilemap>();
        units = FindObjectsOfType<GenericUnit>();

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });

        actionButtons = unitMenu.GetComponentsInChildren<Button>();
    }

    private void Start()
    {
        ai = new AI(this);

        StartCoroutine(lateStart());
    }

    private IEnumerator lateStart()
    {
        yield return new WaitForFixedUpdate();

        AiPickUnit();
    } 

    public void UnitClicked(GenericUnit Unit)
    {
        if (activePlayer == Player.Living && state != State.Attacking)
        {
            return;
        }
        SelectUnit(Unit);
    }

    public void SelectUnit(GenericUnit unit)
    {
        switch (state)
        {
            case State.SelectingUnit:
                if (!unit.CompareTag(activePlayer.ToString()) || !unit.CanBeSelected()) 
                {
                    return;
                }
                selectedUnit = unit;

                int selectionTimer = GetSelectionTimerValue();
                Debug.Log("seleciton timer: " + selectionTimer);
                if (selectionTimer < SELECTIONTIMERMAX)
                {
                    ResetTeamSelectionTimers();
                }
                unit.StartSelectionTimer(selectionTimer);

                unitMenu.enabled = true;
                attackText.text = unit.Attack.ToString() + " Attack";

                if (activePlayer == Player.Living)
                {
                    StartCoroutine(selectedUnit.ApplySelectedShader(delegate () {
                        return state == State.SelectingUnit && activePlayer == Player.Living;
                    }));
                }

                if (activePlayer == Player.Dead)
                {
                    ai.DecideActions(selectedUnit, units);
                }

                break;

            case State.Attacking:
                if (unit.CompareTag(activePlayer.ToString())) 
                {
                    return;
                }
                if (unit.CanBeAttacked() && !selectedUnit.CompareTag(unit.tag) && selectedUnit.UnitInRange(unit))

                {
                    selectedUnit.AttackUnit(unit);

                    //Debug.Log("switching: " + unit.SwitchingSides);
                    //if (unit.SwitchingSides)
                    //{
                    //    DecrementTeamTurnTimers(unit.tag);
                    //}

                    state = State.SelectingUnit;
                    unitMenu.enabled = false;

                    EndTurn();
                }
                break;

            default:
                break;
        }
    }

    public void Attack()
    {
        if (state == State.Attacking)
        {
            return;
        }
        state = State.Attacking;

        GenericUnit[] enemyUnitsInRange = Array.FindAll(units, delegate (GenericUnit target)
        {
            return !selectedUnit.CompareTag(target.tag) && target.CanBeAttacked() && selectedUnit.UnitInRange(target);
        });

        Array.ForEach(enemyUnitsInRange, delegate (GenericUnit enemy)
        {
            StartCoroutine(enemy.ApplyAttackShader(delegate () { 
                return state == State.Attacking && activePlayer == Player.Living;
            }));
        });
    }

    public void Move()
    {
        state = State.Moving;
        ShowTilesInRange(selectedUnit);
    }

    public void EndTurn()
    {
        Debug.Log("Turn has ended!");
        state = State.SelectingUnit;
        unitMenu.enabled = false;

        ChangeTurns();
    }

    public Vector3Int FindClosestTile(Vector3 position)
    {
        int maxZ = tilemap.cellBounds.zMax;
        position.z = maxZ + 1;
        Vector3 positionCopy = position;

        Vector3 closestTile = Vector3.positiveInfinity;
        for (int z = 0; z < maxZ; z++)
        {
            positionCopy.z = z;
            Vector3Int cellPosition = tilemap.layoutGrid.WorldToCell(positionCopy);

            if (tilemap.HasTile(cellPosition))
            {
                Vector3 cellMid = tilemap.layoutGrid.GetCellCenterWorld(cellPosition);
                if (Vector3.Distance(position, cellMid) < Vector3.Distance(position, closestTile))
                {
                    closestTile = cellMid;
                }
            }
        }
        return tilemap.layoutGrid.WorldToCell(closestTile);
    }

    public bool MoveSelectedUnit(Vector3Int cellPosition)
    {
        if (selectedUnit.TileInRange(cellPosition))
        {
            RemoveColorFromTilesInRange(selectedUnit);

            selectedUnit.Move(cellPosition);

            return true;
        }

        return false;
    }

    public bool HasTileAtPosition(Vector3Int position)
    {
        return tilemap.HasTile(position);
    }

    public void UnitEliminated()
    {
        units = Array.FindAll(units, delegate (GenericUnit u)
        {
            return !u.gameObject.activeInHierarchy;
        });
    }

    private Vector3Int GetClickedGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        return FindClosestTile(mouseWorldPos);
    }

    private void ShowTilesInRange(GenericUnit unit)
    {
        foreach(Vector3Int tilePos in unit.TilesInRange())
        {
            tilemap.SetTileFlags(tilePos, TileFlags.None);
            tilemap.SetColor(tilePos, Color.red);
        }

        tilemap.SetTileFlags(new Vector3Int(0, 0, 1), TileFlags.None);
        tilemap.SetColor(new Vector3Int(0,0,0), Color.yellow);
    }

    private void RemoveColorFromTilesInRange(GenericUnit unit)
    {
        foreach(Vector3Int tileInRange in unit.TilesInRange()){
            tilemap.SetColor(tileInRange, new Color(1.0f, 1.0f, 1.0f, 1.0f));
            tilemap.SetTileFlags(tileInRange, TileFlags.LockColor);
        }
    }

    private void ChangeTurns() 
    {
        if (activePlayer == Player.Living) 
        {
            activePlayer = Player.Dead;
            // TODO: Uncomment to turn off buttons for player once AI can play
            // Array.ForEach(actionButtons, delegate (Button b) { b.interactable = false; });

            //Array.ForEach(units, delegate (GenericUnit u) { if (u.CompareTag("Living")) { u.DecrementTurnTimers(); } });
            DecrementTeamTurnTimers("Living");
        }
        else if (activePlayer == Player.Dead) 
        {
            activePlayer = Player.Living;

            // Array.ForEach(actionButtons, delegate (Button b) { b.interactable = true; });

            //Array.ForEach(units, delegate (GenericUnit u) { if (u.CompareTag("Dead")) { u.DecrementTurnTimers(); } })
            DecrementTeamTurnTimers("Dead");
        }
        //Array.ForEach(units, delegate (GenericUnit u) { u.DecrementTurnTimers(); });

        CheckLoseCondition();

        if (activePlayer == Player.Living)
        {
            AiPickUnit();
        }
    }

    private void DecrementTeamTurnTimers(string teamTag)
    {
        Array.ForEach(units, delegate (GenericUnit u) { if (u.CompareTag(teamTag)) { u.DecrementTurnTimers(); } });
        Debug.Log("-1 for every " + teamTag);
    }

    private void ResetTeamSelectionTimers()
    {
        GenericUnit[] activeUnits = Array.FindAll(units, delegate (GenericUnit u) {
            return u.CompareTag(activePlayer.ToString());
        });

        if(activeUnits.Length == 1)
        {
            activeUnits[0].SelectionTimer = 0;
            return;
        }

        Debug.Log("reset");
        Debug.Log("active team members: " + activeUnits.Length);

        if(activeUnits[0].SelectionTimer > activeUnits[1].SelectionTimer)
        {
            activeUnits[0].SelectionTimer = 1;
            activeUnits[1].SelectionTimer = 0;
        }
        else
        {
            activeUnits[0].SelectionTimer = 0;
            activeUnits[1].SelectionTimer = 1;
        }

    }

    private void CheckLoseCondition()
    {
        bool unitsLeft = Array.Exists(units, delegate (GenericUnit u)
        {
            return u.CompareTag(activePlayer.ToString()) && u.CanBeSelected();
        });

        if (!unitsLeft)
        {
            Debug.Log(activePlayer.ToString() + " lose");
        }
    }

    private int GetSelectionTimerValue()
    {
        GenericUnit[] activeUnits = Array.FindAll(units, delegate (GenericUnit u) {
            return u.CompareTag(activePlayer.ToString()) && !u.SwitchingSides;
        });

        //if (livingUnits.Length == 0)
        //{
        //    return -1;
        //}

        //if (livingUnits.Length < 2)
        //{
        //    livingUnits[0].ResetSelectionTimer();
        //    return 1;
        //}
        //else if (livingUnits.Length < 3)
        //{
        //    foreach(GenericUnit u in livingUnits)
        //    {
        //        u.ResetSelectionTimer();
        //    }
        //    return 2;
        //}

        //return 3;

        return Mathf.Min(activeUnits.Length, SELECTIONTIMERMAX);
    }

    private void AiPickUnit()
    {
        GenericUnit[] livingUnits = Array.FindAll(units, delegate (GenericUnit u) {
            return u.CompareTag("Living") && u.CanBeSelected();
        });

        if(livingUnits.Length == 0)
        {
            return;
        }

        ai.PickUnit(livingUnits);
    }
}
