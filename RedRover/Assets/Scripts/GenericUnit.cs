using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

    protected NotificationManager NManager;

    // protected TextMeshProUGUI TurnCountdownDisplay;
    protected MeshRenderer Renderer;

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

        NManager = GetComponentInChildren<NotificationManager>();

        Renderer = gameObject.GetComponent<MeshRenderer>();
        Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;
        Renderer.sortingOrder = 1;

        StartCoroutine(
            Move(Controller.FindClosestTile(transform.position), teleport:true)
        );
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

    public string GetAttackStat()
    {
        if (Attack > BaseAttack)
        {
            return BaseAttack.ToString() + " + " + (Attack - BaseAttack).ToString();
        }
        return BaseAttack.ToString();
    }

    public string GetReachStat()
    {
        if (Reach > BaseReach)
        {
            return BaseReach.ToString() + " + " + (Reach - BaseReach).ToString();
        }
        return BaseReach.ToString();
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

    IEnumerator SmoothTranslation(Vector3 target, float speed)
    {
        float amount = 0;
        Vector3 oldPosition = transform.position;
        while (Vector3.Distance(transform.position, target) > 0.02) {
            amount += Time.fixedDeltaTime * speed;
            transform.position = Vector3.Lerp(oldPosition, target, amount);
            yield return new WaitForFixedUpdate();
        }
    }

    private void FaceDirectionOfMoving(Vector3 target)
    {
        transform.right = target - transform.position;
    }

    public IEnumerator Move(Vector3 cellPosition, Action callback=null, bool teleport=false)
    {
        cellPosition.y = transform.position.y;
        FaceDirectionOfMoving(cellPosition);

        SendAlertForMessaging("Moved", "0");
        if (!teleport)
        {
            yield return StartCoroutine(SmoothTranslation(cellPosition, 2));
        }
        transform.position = cellPosition;

        transform.rotation = CompareTag("Living") ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);

        if (callback != null)
        {
            callback();
        }
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        Health = Mathf.Max(0, Health - value);

        SendAlertForMessaging("WasAttacked", value.ToString());

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

            Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;
        }
    }

    public void DecrementTurnTimers()
    {
        SelectionTimer = Math.Max(0, SelectionTimer - 1);

        if (SwitchingSides)
        {
            SwitchSidesCountdown = Math.Max(0, SwitchSidesCountdown - 1);
        }

        SendAlertForMessaging("ChangingSides", SwitchSidesCountdown.ToString());

        if (SwitchingSides && SwitchSidesCountdown <= 0)
        {
            SwitchingSides = false;
            NumTimesSwitched++;
            tag = CompareTag("Living") ? "Dead" : "Living";
            Renderer.material = CompareTag("Living") ? LivingMaterial : DeadMaterial;

            MaxHealth = Mathf.RoundToInt(InitialMaxHealth * (1 - (NumTimesSwitched / (MaxAllowedSwitches + 1f))));
            Health = MaxHealth;

            SendAlertForMessaging("ChangedSides", Health.ToString());
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

    public IEnumerable<Tile> TilesInRange()
    {
        for (int i=0; i < Controller.GroundTiles.Count;  i++)
        {
            Tile tile = Controller.GroundTiles[i];
            if (TileInRange(tile.transform.position))
            {
                yield return tile;
            }
        }
    }

    public IEnumerable<Tile> TilesInAttackRange(bool showMovement=true)
    {
        for (int i = 0; i < Controller.GroundTiles.Count; i++)
        {
            Tile tile = Controller.GroundTiles[i];
            if (TileInAttackRange(tile.transform.position, showMovement))
            {
                yield return tile;
            }
        }
    }

    public IEnumerator ApplySelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        ShaderActive = true;
        Material material = Renderer.material;
        Shader oldShader = material.shader;

        string shaderName = CompareTag("Living") ? "Shader Graphs/Outline" : "Shader Graphs/DeadOutline";
        Shader shader = Shader.Find(shaderName);
        material.shader = shader;

        material.SetFloat("_Size", 1); // Smaller is bigger
        material.SetColor("_Color", Color.yellow);

        Renderer.material = material;

        yield return new WaitWhile(() => continueWhile());

        material.shader = oldShader;
        material.SetColor("_Color", Color.white);
        material.renderQueue = 3001;

        ShaderActive = false;
    }

    public IEnumerator ApplyCanBeSelectedShader(Func<bool> continueWhile)
    {
        yield return new WaitUntil(() => !ShaderActive);
        ShaderActive = true;
        Material material = Renderer.material;
        Shader oldShader = material.shader;

        string shaderName = CompareTag("Living") ? "Shader Graphs/PulseOutline" : "Shader Graphs/DeadPulseOutline";
        Shader shader = Shader.Find(shaderName);
        material.shader = shader;

        material.SetFloat("_MinValue", 0.3f);
        material.SetFloat("_Speed", 2);
        material.SetFloat("_Size", 1); // Smaller is bigger
        material.SetColor("_Color", Color.yellow);

        Renderer.material = material;

        yield return new WaitWhile(() => continueWhile());

        material.shader = oldShader;
        material.SetColor("_Color", Color.white);
        material.renderQueue = 3001;

        ShaderActive = false;
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

    //----------Send alerts to inform the turn state manager of changes----------
    private void SendAlertForMessaging(string uState, string uValue) 
    {
        if (uState.Equals("Moved")) 
        {
            Controller.GetAlert(gameObject.tag.ToString(), unitName, uState);
        }
        else if (uState.Equals("WasAttacked"))
        {
            NManager.ShowNotification("-", uValue);
            if (Health == 0)
            {
                if (NumTimesSwitched == MaxAllowedSwitches)
                {
                    Controller.GetAlert(gameObject.tag.ToString(), unitName, "WasKilled");
                }
                else 
                {
                    Controller.GetAlert(gameObject.tag.ToString(), unitName, "ChangingSides");
                    NManager.ShowNotification("n", uValue); 
                }
            }
            else Controller.GetAlert(gameObject.tag.ToString(), unitName, uState);
        }
        else if (uState.Equals("ChangingSides"))
        {
            if (uValue.Equals("0")) return;
            NManager.ShowNotification("n", uValue);
        
        }
        else if (uState.Equals("ChangedSides"))
        {
            NManager.ShowNotification("+", uValue);
        }
    }
    //---------------------------------------------------------------------------
}
