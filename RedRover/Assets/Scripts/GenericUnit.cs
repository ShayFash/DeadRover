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
    public int MaxHealth { get; protected set; }
    public int InitialMaxHealth { get; protected set; }
    public HealthBar healthBar;
    


    [SerializeField]
    protected int NumTurnsToSwitchSides = 4;
    [SerializeField]
    protected int MaxAllowedSwitches = 3;
    public int SwitchSidesCountdown { get; protected set; }
    public int NumTimesSwitched { get; protected set; }
    public bool SwitchingSides { get; protected set; }
    public bool IsEliminated { get; protected set; }

    protected Controller Controller;

    protected Tilemap Tilemap;

    protected TextMeshProUGUI TurnCountdownDisplay;
    protected SpriteRenderer Renderer;

    protected TextMeshProUGUI HealthDisplay;

    protected bool ShaderActive = false;

    protected void Init()
    {
        MaxHealth = Health;
        healthBar.SetMaxHealth(MaxHealth);
        InitialMaxHealth = MaxHealth;

        NumTimesSwitched = 0;
        IsEliminated = false;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();
        Tilemap = FindObjectOfType<Tilemap>();

        TextMeshProUGUI[] childTexts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        TurnCountdownDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("TurnCountdown"); });
        HealthDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("HealthStatDisplay"); });


        Renderer = gameObject.GetComponent<SpriteRenderer>();
        Renderer.color = CompareTag("Living") ? Color.white : Color.black;

        UpdateHealthDisplay();

        Move(Controller.FindClosestTile(transform.position));
    }

    public bool CanBeAttacked()
    {
        return !SwitchingSides && !IsEliminated;
    }

    public bool CanBeSelected()
    {
        return !SwitchingSides && !IsEliminated;
    }

    public Vector3Int GetTilePosition()
    {
        return Tilemap.layoutGrid.WorldToCell(transform.position);
    }

    public void AttackUnit(GenericUnit unit)
    {
        Debug.Log("Aaaaattack!");
        unit.TakeDamage(Attack);
    }

    public void Move(Vector3Int cellPosition)
    {
        Vector3 alignedPosition = Tilemap.layoutGrid.GetCellCenterWorld(cellPosition);
        alignedPosition.z += 1;
        transform.position = alignedPosition;
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        Health = Mathf.Max(0, Health - value);
        healthBar.SetHealth(Health);
        UpdateHealthDisplay();

        if (Health == 0)
        {
            if (NumTimesSwitched == MaxAllowedSwitches)
            {
                gameObject.SetActive(false);
                IsEliminated = true;
                return;
            }

            SwitchingSides = true;
            SwitchSidesCountdown = NumTurnsToSwitchSides;

            TurnCountdownDisplay.text = SwitchSidesCountdown.ToString();
            TurnCountdownDisplay.color = CompareTag("Living") ? Color.black : Color.white;
            TurnCountdownDisplay.enabled = true;

            HealthDisplay.enabled = false;
        }
    }

    public void DecrementTurnTimers()
    {
        if (!SwitchingSides)
        {
            return;
        }

        SwitchSidesCountdown--;
        TurnCountdownDisplay.text = SwitchSidesCountdown.ToString();

        if (SwitchSidesCountdown <= 0)
        {
            SwitchingSides = false;
            NumTimesSwitched++;
            tag = CompareTag("Living") ? "Dead" : "Living";
            Renderer.color = CompareTag("Living") ? Color.white : Color.black;

            MaxHealth = Mathf.RoundToInt(InitialMaxHealth * (1 - (NumTimesSwitched / (MaxAllowedSwitches + 1f))));
            Health = MaxHealth;

            TurnCountdownDisplay.enabled = false;

            HealthDisplay.enabled = true;
            UpdateHealthDisplay();
        }
    }

    public bool UnitInRange(GenericUnit unit)
    {
        Vector3Int myTilePosittion = Tilemap.layoutGrid.WorldToCell(transform.position);
        Vector3Int theirTilePosition = Tilemap.layoutGrid.WorldToCell(unit.transform.position);

        int tileDistance = 0;
        for (int i = 0; i <= 1; i++) {
            tileDistance += Mathf.Abs(myTilePosittion[i] - theirTilePosition[i]);
        }

        return tileDistance <= Reach;
    }

    public IEnumerable<GenericUnit> UnitsInRange(IEnumerable<GenericUnit> units)
    {
        IEnumerable<GenericUnit> inReach = from unit in units where UnitInRange(unit) select unit;

        return inReach;
    }

    public bool TileInRange(Vector3Int tilePosition)
    {
        Vector3Int myTilePosition = Tilemap.layoutGrid.WorldToCell(transform.position);

        int tileDistance = 0;
        for (int i = 0; i <= 1; i++)
        {
            tileDistance += Mathf.Abs(myTilePosition[i] - tilePosition[i]);
        }

        return tileDistance <= Movement;
    }
    public IEnumerable<Vector3Int> TilesInRange()
    {
        foreach (Vector3Int position in Tilemap.cellBounds.allPositionsWithin)
        {
            if (TileInRange(position))
            {
                yield return position;
            }
        }
    }

    public IEnumerator ApplyAttackShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        ShaderActive = true;
        Material oldMaterial = Renderer.material;

        Material material = new Material(Shader.Find("Shader Graphs/PulseHighlight"));
        material.color = Color.red;
        material.SetFloat("_Intensity", 0.5f);
        material.SetFloat("_Speed", 3);
        material.SetFloat("_TimeElapsed", 0);

        Renderer.material = material;

        float timeElasped = 0;

        while (continueWhile())
        {
            timeElasped += Time.deltaTime;
            // This is inefficient for a lot of materials, but it won't matter for this game
            Renderer.material.SetFloat("_TimeElapsed", timeElasped);

            yield return new WaitForEndOfFrame();
        }

        Renderer.material = oldMaterial;

        ShaderActive = false;
    }

    public IEnumerator ApplySelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        ShaderActive = true;
        Material oldMaterial = Renderer.material;

        Material material = new Material(Shader.Find("Shader Graphs/Highlight"));
        material.color = Color.yellow;
        material.SetFloat("_Intensity", 0.5f);

        Renderer.material = material;

        yield return new WaitWhile(() => continueWhile());

        Renderer.material = oldMaterial;
        ShaderActive = false;
    }

    public IEnumerator ApplyCanBeSelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        ShaderActive = true;
        Material oldMaterial = Renderer.material;

        Material material = new Material(Shader.Find("Shader Graphs/PulseHighlight"));
        material.color = Color.yellow;
        material.SetFloat("_Intensity", 0.5f);
        material.SetFloat("_Speed", 3);
        material.SetFloat("_TimeElapsed", 0);

        Renderer.material = material;

        float timeElasped = 0;

        while (continueWhile())
        {
            timeElasped += Time.deltaTime;
            // This is inefficient for a lot of materials, but it won't matter for this game
            Renderer.material.SetFloat("_TimeElapsed", timeElasped);

            yield return new WaitForEndOfFrame();
        }

        Renderer.material = oldMaterial;
        ShaderActive = false;
    }

    private void OnMouseDown()
    {
        Controller.UnitClicked(this);
        
    }

    private void UpdateHealthDisplay()
    {
        HealthDisplay.text = Health.ToString() + "/" + MaxHealth.ToString() + " HP";
    }
}
