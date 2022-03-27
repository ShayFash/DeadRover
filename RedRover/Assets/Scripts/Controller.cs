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
        Waiting,
        Attacking,
        Moving
    }

    private enum Player
    {
        Living,
        Dead
    }

    private State state = State.Waiting;
    private Player activePlayer = Player.Living;   // Living starts
    
    private GenericUnit selectedUnit;
    private GenericUnit[] units;

    private Tilemap tilemap;

    private Canvas unitMenu;
    private TextMeshProUGUI rangeText;
    private TextMeshProUGUI attackText;
    private TextMeshProUGUI healthNumText;
    private TextMeshProUGUI unitNameText;
    private Button[] actionButtons;
    private Button moveButton;
 
    public GameObject[] WinPanel;
    public GameObject[] LossPanel;
    private GameObject WinScreen;
    private GameObject LossScreen;

    public GameObject HelpPanel;

    private AI ai;

    private UnitLink[] linkObjectPool = new UnitLink[10];
    public GameObject linkObject;

    private Dictionary<string, string> linkPairNames = new Dictionary<string, string>
    {
        {"Bear", "Rabbit"}, {"Owl", "Fox"}, {"Deer", "Deer"},
        {"Rabbit", "Bear"}, {"Fox", "Owl"}
    };

    private void Awake()
    {
        tilemap = GameObject.FindGameObjectWithTag("Ground").GetComponent<Tilemap>();
        units = FindObjectsOfType<GenericUnit>();

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        rangeText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("RangeStatDisplay");
        });
        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });
        unitNameText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("UnitName");
        });

        healthNumText = GameObject.FindGameObjectWithTag("HealthBar").GetComponentInChildren<TextMeshProUGUI>();

        Button[] buttons = unitMenu.GetComponentsInChildren<Button>();
        actionButtons = Array.FindAll(buttons, delegate (Button b)
        {
            return b.CompareTag("ActionButton") || b.CompareTag("MoveButton");
        });
        moveButton = Array.Find(actionButtons, delegate (Button b)
        {
            return b.CompareTag("MoveButton");
        });

        UnitLink temp;
        for (int i = 0; i < linkObjectPool.Length; i++)
        {
            temp = Instantiate(linkObject).GetComponent<UnitLink>();
            temp.HideLink();
            linkObjectPool[i] = temp;
        }
    }

    private void Start()
    {
        ai = new AI(this);

        StartCoroutine(lateStart());

        WinPanel = GameObject.FindGameObjectsWithTag("Win");
        LossPanel = GameObject.FindGameObjectsWithTag("Loss");

        WinScreen = GameObject.Find("WinScreen");
        LossScreen = GameObject.Find("LossScreen");

        // ShowHelpPanel();
    }

    private IEnumerator lateStart()
    {
        yield return new WaitForFixedUpdate();

        UpdateLinks();
        ChangeStateToSelecting();
        AiPickUnit();
    } 

    public void UnitClicked(GenericUnit Unit)
    {
        if ((activePlayer == Player.Living && state != State.Attacking) || (activePlayer == Player.Dead && state != State.SelectingUnit))
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
                state = State.Waiting;

                selectedUnit.WasSelected();

                unitMenu.enabled = true;
                SetRangeAndAttackText(unit);

                Player currentPlayer = activePlayer;
                StartCoroutine(selectedUnit.ApplySelectedShader(delegate () {
                    return activePlayer == currentPlayer;
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
                    UpdateLinks();

                    EndTurn();
                }
                break;

            default:
                break;
        }
    }

    public void SetRangeAndAttackText(GenericUnit unit)
    {
        if(unit == null)
        {
            rangeText.text = "Range: - ";
            attackText.text = "Attack: - ";
            healthNumText.text = "-/-";
            unitNameText.text = "";

            return;
        }

        rangeText.text = "Range: " + unit.Reach.ToString();
        attackText.text = "Attack: " + unit.Attack.ToString();
        healthNumText.text =  unit.Health.ToString() + " / " + unit.MaxHealth.ToString();
        unitNameText.text = unit.unitName;
    }

    public void ResetRangeAndAttackText()
    {
        SetRangeAndAttackText(selectedUnit);   
    }

    public void Attack()
    {
        if (state == State.Attacking || selectedUnit == null)
        {
            return;
        }
        
        state = State.Attacking;
        //Clear, if movement tiles are still highlighted
        RemoveColorFromTilesInRange(selectedUnit);

        //Show tiles in attack range
        ShowTilesInRange(selectedUnit, true, false);

        Player currentPlayer = activePlayer;
        for (int i=0; i < units.Length; i++)
        {
            GenericUnit target = units[i];

            bool isTargetInRange = !selectedUnit.CompareTag(target.tag) && target.CanBeAttacked() && selectedUnit.UnitInRange(target);
            if (isTargetInRange)
            {
                StartCoroutine(target.ApplyAttackShader(delegate () {
                    return state == State.Attacking && activePlayer == currentPlayer;
                }));
            }
        }
    }

    public void Move()
    {
        if (selectedUnit == null)
        {
            return;
        }
        state = State.Moving;
        //Clear, if attack tiles are still highlighted
        RemoveColorFromTilesInRange(selectedUnit);
        ShowTilesInRange(selectedUnit);
    }

    public void TileClicked(Vector3 position)
    {
        // TODO: need something to cancel move if you don't want to move now
        if (state == State.Moving && activePlayer == Player.Living)
        {
            bool moved = TryMoveSelectedUnit(position);
            if (moved)
            {
                moveButton.interactable = false;
                state = State.Waiting;
            }
        }
    }

    public void EndTurn()
    {
        if(selectedUnit == null)
        {
            return;
        }
        state = State.SelectingUnit;
        RemoveColorFromTilesInRange(selectedUnit);
        ChangeTurns();
    }
    public Vector3 FindClosestTile(Vector3 position)
    {
        Vector3 closestTile = Vector3.positiveInfinity;
        float smallestDistance = Mathf.Infinity;
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            Vector3 tilePosition = tilemap.transform.GetChild(i).position;
            float distance = 0;
            distance += Mathf.Abs(position[0] - tilePosition[0]);
            distance += Mathf.Abs(position[2] - tilePosition[2]);

            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestTile = tilePosition;
            }
        }
        return closestTile;
    }

    public bool TryMoveSelectedUnit(Vector3 cellPosition)
    {
        if (selectedUnit.TileInRange(cellPosition) && !TileOccupied(cellPosition))
        {
            state = State.Waiting;
            RemoveColorFromTilesInRange(selectedUnit);
            selectedUnit.Move(cellPosition);
            UpdateLinks();
            return true;
        }
        return false;
    }

    public bool TileOccupied(Vector3 cellPosition)
    {
        foreach(GenericUnit u in units)
        {
            Vector3 unitPosition = u.GetTilePosition();

            if(Mathf.Approximately(unitPosition.x, cellPosition.x) && Mathf.Approximately(unitPosition.z, cellPosition.z))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasTileAtPosition(Vector3 position)
    {
        for (int i=0; i < tilemap.transform.childCount; i++)
        {
            Vector3 tilePosition = tilemap.transform.GetChild(i).position;
            if (Mathf.Approximately(position.x, tilePosition.x) && Mathf.Approximately(position.z, tilePosition.z))
            {
                return true;
            }
        }
        return false;
    }

    private void ChangeStateToSelecting()
    {
        selectedUnit = null;
        state = State.SelectingUnit;

        for (int i=0; i < units.Length; i++)
        {
            GenericUnit unit = units[i];

            if (unit.CompareTag(activePlayer.ToString()) && unit.CanBeSelected())
            {
                Player currentPlayer = activePlayer;
                StartCoroutine(unit.ApplyCanBeSelectedShader(delegate () {
                    return state == State.SelectingUnit && activePlayer == currentPlayer;
                }));
            }
        }
    }

    public void ShowTilesInRange(GenericUnit unit, bool showAttack=false, bool showMovement = true)
    {
        // TODO: fix showing range
        //if (showAttack)
        //{
        //    foreach (GameObject tile in unit.TilesInAttackRange(showMovement))
        //    {
        //        tile.SetActive(false);
        //    }
        //}

        //if (showMovement)
        //{
        //    foreach (GameObject tile in unit.TilesInRange())
        //    {
        //        tile.SetActive(false);
        //    }
        //}
    }

    public void RemoveColorFromTilesInRange(GenericUnit unit)
    {
        // TODO: fix showing range

        // Remove movement color
        //foreach (GameObject tile in unit.TilesInRange())
        //{
        //    tile.SetActive(false);
        //}

        ////Remove attack color
        //foreach (GameObject tile in unit.TilesInAttackRange())
        //{
        //    tile.SetActive(false);
        //}

        ////Recolor what gets deleted by other units
        //if (state == State.Moving)
        //{
        //    ShowTilesInRange(selectedUnit);
        //}
        //else if (state == State.Attacking)
        //{
        //    ShowTilesInRange(selectedUnit, true, false);
        //}
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

        DecrementTurnTimers();

        CheckLoseCondition();

        ChangeStateToSelecting();

        if (activePlayer == Player.Living)
        {
            AiPickUnit();
        }
    }

    private void DecrementTurnTimers()
    {
        Array.ForEach(units, delegate (GenericUnit u) { u.DecrementTurnTimers(); });
        UpdateLinks();

        GenericUnit[] activePlayerUnits = Array.FindAll(units, delegate (GenericUnit u)
        {
            return u.CompareTag(activePlayer.ToString()) && u.IsActive();
        });

        GenericUnit nextSelectableUnit = null;
        int lowestSelectionTimer = 100000;

        // If no unit is selectable due to turn timers, just set the lowest turn timer to 0
        for (int i=0; i < activePlayerUnits.Length; i++)
        {
            GenericUnit unit = activePlayerUnits[i];
            if (unit.CanBeSelected())
            {
                return;
            } else if (!unit.SwitchingSides && unit.SelectionTimer < lowestSelectionTimer)
            {
                lowestSelectionTimer = unit.SelectionTimer;
                nextSelectableUnit = unit;
            }
        }

        // If it was still null, there are no selectable units and no units that aren't switching sides
        // So the active player should lose
        if (nextSelectableUnit != null)
        {
            nextSelectableUnit.ResetSelectionTimer();
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

            if(activePlayer == Player.Living)
            {
                LossScreen.gameObject.SetActive(true);
            }
            else
            {
                WinScreen.gameObject.SetActive(true);
            }
        }
    }

    private void AiPickUnit()
    {
        GenericUnit[] selectableUnits = Array.FindAll(units, delegate (GenericUnit u) {
            return u.CompareTag(activePlayer.ToString()) && u.CanBeSelected();
        });
        if (selectableUnits.Length <= 0)
        {
            return;
        }
        StartCoroutine(ai.PickUnit(selectableUnits));
    }

    private void UpdateLinks()
    {
        void updateForTeam(string team)
        {
            GenericUnit[] activeTeamUnits = Array.FindAll(units, delegate (GenericUnit u)
            {
                return u.CompareTag(team) && u.IsActive();
            });

            for (int i = 0; i < activeTeamUnits.Length; i++)
            {
                GenericUnit unit = activeTeamUnits[i];

                for (int j = i-1; j >= 0; j--)
                {
                    GenericUnit otherUnit = activeTeamUnits[j];

                    if (linkPairNames[unit.unitName] == otherUnit.unitName && !(unit.linked && otherUnit.linked))
                    {
                        for (int l = 0; l < linkObjectPool.Length; l++)
                        {
                            if (!linkObjectPool[l].AlreadyConnected())
                            {
                                linkObjectPool[l].SetLink(unit, otherUnit);
                                break;
                            }
                        }
                    }
                }
            }
        }

        updateForTeam(Player.Living.ToString());
        updateForTeam(Player.Dead.ToString());
    }

    public void ShowHelpPanel() 
    {
        HelpPanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideHelpPanel() 
    {
        HelpPanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
