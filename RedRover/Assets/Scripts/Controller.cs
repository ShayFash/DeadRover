using System;
using System.Collections;
using System.Collections.Generic;
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

    private List<GenericUnit> livingLinks;
    private List<GenericUnit> deadLinks;
    private List<GenericUnit> linksWaitList;

    private Tilemap tilemap;

    private Canvas unitMenu;
    private TextMeshProUGUI rangeText;
    private TextMeshProUGUI attackText;
    private Button[] actionButtons;

    private AI ai;

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

        UpdateUnitLinks();
    }

    private void Awake()
    {
        tilemap = FindObjectOfType<Tilemap>();
        units = FindObjectsOfType<GenericUnit>();

        livingLinks = new List<GenericUnit>();
        deadLinks = new List<GenericUnit>();
        linksWaitList = new List<GenericUnit>();
        for (int i = 0; i < units.Length; i++) 
        {
            if (units[i].tag == "Living") livingLinks.Add(units[i]);
            else if (units[i].tag == "Dead") deadLinks.Add(units[i]);
        }

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        rangeText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("RangeStatDisplay");
        });
        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });

        

        Button[] buttons = unitMenu.GetComponentsInChildren<Button>();
        actionButtons = Array.FindAll(buttons, delegate (Button b)
        {
            return b.CompareTag("ActionButton");
        });
    }

    private void Start()
    {
        ai = new AI(this);

        StartCoroutine(lateStart());
    }

    private IEnumerator lateStart()
    {
        yield return new WaitForFixedUpdate();

        aiPickUnit();
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

                unitMenu.enabled = true;
                rangeText.text = "Range: " + unit.Reach.ToString();
                attackText.text = "Attack: " + unit.Attack.ToString();

                Player currentPlayer = activePlayer;
                StartCoroutine(selectedUnit.ApplySelectedShader(delegate () {
                    return state == State.SelectingUnit && activePlayer == currentPlayer;
                }));

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

                    EndTurn();

                    state = State.SelectingUnit;
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

        Player currentPlayer = activePlayer;

        Array.ForEach(enemyUnitsInRange, delegate (GenericUnit enemy)
        {
            StartCoroutine(enemy.ApplyAttackShader(delegate () { 
                return state == State.Attacking && activePlayer == currentPlayer;
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
            Array.ForEach(actionButtons, delegate (Button b) { b.interactable = false; });
        }
        else if (activePlayer == Player.Dead) 
        {
            activePlayer = Player.Living;
            Array.ForEach(actionButtons, delegate (Button b) { b.interactable = true; });
        }
        Array.ForEach(units, delegate (GenericUnit u) { u.DecrementTurnTimers(); });

        checkLoseCondition();

        if (activePlayer == Player.Living)
        {
            aiPickUnit();
        }
    }

    private void checkLoseCondition()
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

    private void aiPickUnit()
    {
        GenericUnit[] livingUnits = Array.FindAll(units, delegate (GenericUnit u) {
            return u.CompareTag("Living") && u.CanBeSelected();
        });
        ai.PickUnit(livingUnits);
    }


    //----Beginning-of-the-code-for-the-links--------------------------------------------

    private void UpdateUnitLinks() 
    {
        CheckLinks(livingLinks);
        CheckLinks(deadLinks);

        CheckWaitList();

        DesignateLinks(livingLinks);
        DesignateLinks(deadLinks);
    }

    private void DesignateLinks(List<GenericUnit> unitList) 
    {
        for(int i = 0; i < unitList.Count; i++) 
        {
            UnitLink link = unitList[i].GetComponent<UnitLink>();
            if (i == unitList.Count-1) link.SetLink(null);
            else link.SetLink(unitList[i+1]);
        }
    }

    private void CheckLinks(List<GenericUnit> unitList) 
    {
        for(int i = 0; i < unitList.Count; i++) 
        {
            UnitLink link = unitList[i].GetComponent<UnitLink>();
            if (unitList[i].IsEliminated) RemoveLink(unitList, unitList[i]);
            else if (unitList[i].SwitchingSides) 
            {
                AddLink(linksWaitList, unitList[i]);
                RemoveLink(unitList, unitList[i]);
            }
        }
    }

    private void CheckWaitList() 
    {
        for(int i = 0; i < linksWaitList.Count; i++) 
        {
            UnitLink link = linksWaitList[i].GetComponent<UnitLink>();
            if (!linksWaitList[i].SwitchingSides && linksWaitList[i].tag == "Living") 
            {
                AddLink(livingLinks, linksWaitList[i]);
                RemoveLink(linksWaitList, linksWaitList[i]);
            } 
            else if (!linksWaitList[i].SwitchingSides && linksWaitList[i].tag == "Dead") 
            {
                AddLink(deadLinks, linksWaitList[i]);
                RemoveLink(linksWaitList, linksWaitList[i]);
            }
        }
    }

    private void AddLink(List<GenericUnit> unitList, GenericUnit unit) 
    {
        unitList.Add(unit);
    }

    private void RemoveLink(List<GenericUnit> unitList, GenericUnit unit) 
    {
        int index = unitList.IndexOf(unit);
        unitList.RemoveAt(index);
    }

    //----End-of-the-code-for-the-links-------------------------------------------------
}
