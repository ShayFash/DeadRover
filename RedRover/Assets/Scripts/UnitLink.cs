using System;
using System.Collections;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    public int MaxTileDistance = 2;
    private LineRenderer lineRenderer;

    private GenericUnit unit1;
    private GenericUnit unit2;

    private Gradient LivingGradient;
    private Gradient DeadGradient;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        DeadGradient = new Gradient();
        GradientColorKey[] colourKeys = new GradientColorKey[2];
        colourKeys[0].color = new Color(0.453f, 0.043f, 1, 1);
        colourKeys[0].time = 0;

        colourKeys[1].color = new Color(0.945f, 0.302f, 1, 1);
        colourKeys[1].time = 1;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1;
        alphaKeys[0].time = 0;

        alphaKeys[1].alpha = 1;
        alphaKeys[1].time = 1;

        DeadGradient.SetKeys(colourKeys, alphaKeys);


        LivingGradient = new Gradient();
        colourKeys = new GradientColorKey[2];
        colourKeys[0].color = new Color(1, 0.710f, 0.125f, 1);
        colourKeys[0].time = 0;

        colourKeys[1].color = new Color(1, 0.960f, 0.302f, 1);
        colourKeys[1].time = 1;

        LivingGradient.SetKeys(colourKeys, alphaKeys);

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    public void SetLink(GenericUnit unit1, GenericUnit unit2) 
    {
        this.unit1 = unit1;
        this.unit2 = unit2;
        if (unit1.CompareTag("Living"))
        {
            lineRenderer.colorGradient = LivingGradient;
        } else if (unit1.CompareTag("Dead"))
        {
            lineRenderer.colorGradient = DeadGradient;
        }
        lineRenderer.enabled = true;
        StartCoroutine(FollowTransform());
    }

    public bool AlreadyConnected()
    {
        return lineRenderer.enabled;
    }

    public void HideLink()
    {
        lineRenderer.enabled = false;
    }

    private bool LinkConditionsMet()
    {
        Vector3 unit1TilePosition = unit1.GetTilePosition();
        Vector3 unit2TilePosition = unit2.GetTilePosition();

        float tileDistance = 0;
        tileDistance += Mathf.Abs(unit1TilePosition[0] - unit2TilePosition[0]);
        tileDistance += Mathf.Abs(unit1TilePosition[2] - unit2TilePosition[2]);

        bool unitsActive = unit1.IsActive() && unit2.IsActive();
        return lineRenderer.enabled && unit1.CompareTag(unit2.tag) && unitsActive && tileDistance <= MaxTileDistance;
    }

    private IEnumerator FollowTransform()
    {
        unit1.linked = true;
        unit2.linked = true;
        if (LinkConditionsMet())
        {
            unit1.Buff(unit2.BaseAttack, unit2.BaseMovement, Math.Max(unit2.BaseReach, unit1.BaseReach));
            unit2.Buff(unit1.BaseAttack, unit1.BaseMovement, Math.Max(unit2.BaseReach, unit1.BaseReach));
        }

        while (LinkConditionsMet())
        {
            Vector3 position = unit1.transform.position;
            position.y = Mathf.Max(position.y, 0.3f);
            lineRenderer.SetPosition(0, position);

            position = unit2.transform.position;
            position.y = Mathf.Max(position.y, 0.3f);
            lineRenderer.SetPosition(1, position);


            yield return new WaitForEndOfFrame();
        }
        unit1.RemoveBuff();
        unit2.RemoveBuff();
        unit1.linked = false;
        unit2.linked = false;


        HideLink();
    }
}
