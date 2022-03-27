using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class GenericUnit : MonoBehaviour
{
    public int Reach;
    public int Attack;
    public int Health;
    public int Movement;

    public int BaseReach { get; protected set; }
    public int BaseAttack { get; protected set; }
    public int BaseMovement { get; protected set; }

    public int MaxHealth { get; protected set; }
    public int InitialMaxHealth { get; protected set; }

    public Material LivingMaterial;
    public Material DeadMaterial;

    public bool IsEliminated { get; protected set; }

    public string unitName;


    [SerializeField]
    protected int NumTurnsToSwitchSides = 4;
    [SerializeField]
    protected int MaxAllowedSwitches = 3;
    public int SwitchSidesCountdown { get; protected set; }
    public int NumTimesSwitched { get; protected set; }
    public bool SwitchingSides { get; protected set; }

    // This will be decremented every turn, including the one the unit is picked on and enemy turns
    [SerializeField]
    protected int NumTurnsBetweenSelection = 6;

    public int SelectionTimer { get; protected set; }

    [HideInInspector]
    public bool linked = false;

    protected Controller Controller;

    protected Tilemap Tilemap;

    // protected TextMeshProUGUI TurnCountdownDisplay;
    protected MeshRenderer Renderer;

    protected TextMeshProUGUI HealthDisplay;

    protected bool ShaderActive = false;
    protected bool MouseOver = false;

    protected void Init()
    {
        BaseAttack = Attack;
        BaseReach = Reach;
        BaseMovement = Movement;

        MaxHealth = Health;
        InitialMaxHealth = MaxHealth;

        NumTimesSwitched = 0;
        IsEliminated = false;

        SelectionTimer = 0;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();
        Tilemap = GameObject.FindGameObjectWithTag("Ground").GetComponent<Tilemap>();

        TextMeshProUGUI[] childTexts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        // TurnCountdownDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("TurnCountdown"); });
        HealthDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("HealthStatDisplay"); });

        Renderer = gameObject.GetComponent<MeshRenderer>();
        Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;

        UpdateHealthDisplay();

        Move(Controller.FindClosestTile(transform.position));
    }

    public bool CanBeAttacked()
    {
        return !SwitchingSides && !IsEliminated;
    }

    public bool CanBeSelected()
    {
        return !SwitchingSides && SelectionTimer <= 0 && !IsEliminated;
    }

    public bool IsActive()
    {
        return !SwitchingSides && !IsEliminated;
    }

    public void WasSelected()
    {
        SelectionTimer = NumTurnsBetweenSelection;
    }

    public void ResetSelectionTimer()
    {
        SelectionTimer = 0;
    }

    public bool IsActiveUnit()
    {
        return !SwitchingSides && !IsEliminated;
    }

    public void Buff(int attack, int movement, int reach)
    {
        if (Attack > BaseAttack || Reach > BaseReach || Movement > BaseMovement)
        {
            return;
        }
        Attack = BaseAttack + attack;
        Reach = reach;
        Movement = BaseMovement + movement;
    }

    public void RemoveBuff()
    {
        Attack = BaseAttack;
        Reach = BaseReach;
        Movement = BaseMovement;
    }

    public Vector3 GetTilePosition()
    {
        return transform.position;
    }

    public void AttackUnit(GenericUnit unit)
    {
        Debug.Log("Aaaaattack!");
        unit.TakeDamage(Attack);
    }

    public void Move(Vector3 cellPosition)
    {
        cellPosition.y = transform.position.y;
        transform.position = cellPosition;
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        Health = Mathf.Max(0, Health - value);
        UpdateHealthDisplay();

        if (Health == 0)
        {
            if (NumTimesSwitched == MaxAllowedSwitches)
            {
                gameObject.SetActive(false);
                IsEliminated = true;
                transform.position = new Vector3(100, 100, 100);
                return;
            }

            SwitchingSides = true;
            SwitchSidesCountdown = NumTurnsToSwitchSides;

            ResetSelectionTimer();

            // TurnCountdownDisplay.text = SwitchSidesCountdown.ToString();
            Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;
            // TurnCountdownDisplay.enabled = true;

            HealthDisplay.enabled = false;
        }
    }

    public void DecrementTurnTimers()
    {
        SelectionTimer = Math.Max(0, SelectionTimer - 1);

        if (SwitchingSides)
        {
            SwitchSidesCountdown = Math.Max(0, SwitchSidesCountdown - 1);
        }


        // TurnCountdownDisplay.text = SwitchSidesCountdown.ToString();

        if (SwitchingSides && SwitchSidesCountdown <= 0)
        {
            SwitchingSides = false;
            NumTimesSwitched++;
            tag = CompareTag("Living") ? "Dead" : "Living";
            Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;

            MaxHealth = Mathf.RoundToInt(InitialMaxHealth * (1 - (NumTimesSwitched / (MaxAllowedSwitches + 1f))));
            Health = MaxHealth;

            // TurnCountdownDisplay.enabled = false;

            HealthDisplay.enabled = true;
            UpdateHealthDisplay();
        }
    }

    public bool UnitInRange(GenericUnit unit)
    {

        float tileDistance = 0;
        tileDistance += Mathf.Abs(GetTilePosition()[0] - unit.GetTilePosition()[0]);
        tileDistance += Mathf.Abs(GetTilePosition()[2] - unit.GetTilePosition()[2]);

        return tileDistance <= Reach;
    }

    public IEnumerable<GenericUnit> UnitsInRange(IEnumerable<GenericUnit> units)
    {
        IEnumerable<GenericUnit> inReach = from unit in units where UnitInRange(unit) select unit;

        return inReach;
    }

    public bool TileInRange(Vector3 tilePosition)
    {
        float tileDistance = 0;
        tileDistance += Mathf.Abs(GetTilePosition()[0] - tilePosition[0]);
        tileDistance += Mathf.Abs(GetTilePosition()[2] - tilePosition[2]);

        return tileDistance <= Movement;
    }

    public bool TileInAttackRange(Vector3 tilePosition, bool showMovement)
    {
        float tileDistance = 0;
        tileDistance += Mathf.Abs(GetTilePosition()[0] - tilePosition[0]);
        tileDistance += Mathf.Abs(GetTilePosition()[2] - tilePosition[2]);

        if (!showMovement)
        {
            return tileDistance <= Reach;
        }

        return tileDistance > Movement && tileDistance <= Movement+Reach;
    }

    public IEnumerable<GameObject> TilesInRange()
    {
        for (int i=0; i < Tilemap.transform.childCount;  i++)
        {
            Transform tile = Tilemap.transform.GetChild(i);
            if (TileInRange(tile.position))
            {
                yield return tile.gameObject;
            }
        }
    }

    public IEnumerable<GameObject> TilesInAttackRange(bool showMovement=true)
    {
        for (int i = 0; i < Tilemap.transform.childCount; i++)
        {
            Transform tile = Tilemap.transform.GetChild(i);
            if (TileInAttackRange(tile.position, showMovement))
            {
                yield return tile.gameObject;
            }
        }
    }

    public IEnumerator ApplyAttackShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        //ShaderActive = true;
        //Material oldMaterial = Renderer.material;

        //Material material = new Material(Shader.Find("Shader Graphs/PulseHighlight"));
        //material.color = Color.red;
        //material.SetFloat("_Intensity", 0.5f);
        //material.SetFloat("_Speed", 3);
        //material.SetFloat("_TimeElapsed", 0);

        //Renderer.material = material;

        //float timeElasped = 0;

        //while (continueWhile())
        //{
        //    timeElasped += Time.deltaTime;
        //    // This is inefficient for a lot of materials, but it won't matter for this game
        //    Renderer.material.SetFloat("_TimeElapsed", timeElasped);

        //    yield return new WaitForEndOfFrame();
        //}

        //Renderer.material = oldMaterial;

        //ShaderActive = false;
    }

    public IEnumerator ApplySelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        //ShaderActive = true;
        //Material oldMaterial = Renderer.material;

        //Material material = new Material(Shader.Find("Shader Graphs/Highlight"));
        //material.color = Color.yellow;
        //material.SetFloat("_Intensity", 0.5f);

        //Renderer.material = material;

        //yield return new WaitWhile(() => continueWhile());

        //Renderer.material = oldMaterial;
        //ShaderActive = false;
    }

    public IEnumerator ApplyCanBeSelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        //ShaderActive = true;
        //Material oldMaterial = Renderer.material;

        //Material material = new Material(Shader.Find("Shader Graphs/PulseHighlight"));
        //material.color = Color.yellow;
        //material.SetFloat("_Intensity", 0.5f);
        //material.SetFloat("_Speed", 3);
        //material.SetFloat("_TimeElapsed", 0);

        //Renderer.material = material;

        //float timeElasped = 0;

        //while (continueWhile())
        //{
        //    timeElasped += Time.deltaTime;
        //    // This is inefficient for a lot of materials, but it won't matter for this game
        //    Renderer.material.SetFloat("_TimeElapsed", timeElasped);

        //    if (MouseOver && Renderer.material.color == Color.yellow)
        //    {
        //        Renderer.material.color = Color.green;
        //    } else if (!MouseOver && Renderer.material.color == Color.green)
        //    {
        //        Renderer.material.color = Color.yellow;
        //    }

        //    yield return new WaitForEndOfFrame();
        //}

        //Renderer.material = oldMaterial;
        //ShaderActive = false;
    }

    private void DisplayDetailedInformation()
    {
        DisplayStats();
        DisplayMoveAndAttackRange();
    }

    private void RemoveDetailedInformation()
    {
        RemoveStatsDisplay();
        RemoveMoveAndAttackDisplay();
    }

    private void DisplayStats()
    {
        Controller.SetRangeAndAttackText(this);
    }

    private void RemoveStatsDisplay()
    {
        Controller.ResetRangeAndAttackText();
    }

    private void DisplayMoveAndAttackRange()
    {
        Controller.ShowTilesInRange(this, true);
    }

    private void RemoveMoveAndAttackDisplay()
    {
        Controller.RemoveColorFromTilesInRange(this);
    }

    private void OnMouseEnter()
    {
        if (!IsEliminated)
        {
            MouseOver = true;
            DisplayDetailedInformation();
        }
    }

    private void OnMouseExit()
    {
        MouseOver = false;
        RemoveDetailedInformation();
    }

    private void OnMouseDown()
    {
        Controller.UnitClicked(this);
        
    }

    private void UpdateHealthDisplay()
    {
        // HealthDisplay.text = Health.ToString() + "/" + MaxHealth.ToString() + " HP";
    }

}
