using System;
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
    public int MaxHealth { get; protected set; }
    protected int InitialMaxHealth;

    [SerializeField]
    protected int SwitchSidesCountdown;
    protected int NumTurnsToSwitchSides;
    protected int NumTimesSwitched = 0;
    protected int MaxAllowedSwitches = 3;
    protected bool SwitchingSides;


    protected Controller Controller;

    protected Tilemap Tilemap;

    protected TextMeshProUGUI TurnCountdownDisplay;
    protected SpriteRenderer Sprite;

    protected TextMeshProUGUI HealthDisplay;

    protected void Init()
    {
        MaxHealth = Health;
        InitialMaxHealth = MaxHealth;
        NumTurnsToSwitchSides = SwitchSidesCountdown;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();
        Tilemap = FindObjectOfType<Tilemap>();

        TextMeshProUGUI[] childTexts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        TurnCountdownDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("TurnCountdown"); });
        HealthDisplay = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("HealthStatDisplay"); });


        Sprite = gameObject.GetComponent<SpriteRenderer>();
        Sprite.color = CompareTag("Living") ? Color.white : Color.black;

        UpdateHealthDisplay();

        Vector3Int myTilePosittion = Tilemap.layoutGrid.WorldToCell(transform.position);
        Vector3 alignedPosition = Tilemap.layoutGrid.GetCellCenterWorld(myTilePosittion);
        alignedPosition.y -= 0.1f;
        transform.position = alignedPosition;
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
        UpdateHealthDisplay();

        if (Health == 0)
        {
            if (NumTimesSwitched == MaxAllowedSwitches)
            {
                gameObject.SetActive(false);
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

    public bool CanBeAttacked()
    {
        return !SwitchingSides;
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
            Sprite.color = CompareTag("Living") ? Color.white : Color.black;

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

    public bool TileInRange(Vector3Int clickedTilePosition)
    {
        Vector3Int myTilePosition = Tilemap.layoutGrid.WorldToCell(transform.position);

        Debug.Log("my: " + myTilePosition + " clicked: " + clickedTilePosition);

        int tileDistance = 0;
        for (int i = 0; i <= 1; i++)
        {
            tileDistance += Mathf.Abs(myTilePosition[i] - clickedTilePosition[i]);
        }

        Debug.Log("dist: " + tileDistance);

        return tileDistance <= Reach;
    }

    public IEnumerable<GenericUnit> UnitsInRange(IEnumerable<GenericUnit> units)
    {
        IEnumerable<GenericUnit> inReach = from unit in units where UnitInRange(unit) select unit;

        return  inReach;
    }

    private void OnMouseDown()
    {
        Controller.SelectUnit(this);
    }

    private void UpdateHealthDisplay()
    {
        HealthDisplay.text = Health.ToString() + "/" + MaxHealth.ToString() + " HP";
    }
}
