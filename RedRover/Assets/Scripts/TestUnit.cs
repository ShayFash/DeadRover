using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : GenericUnit
{

    // Start is called before the first frame update
    void Start()
    {
        base.health = 100;
        base.damageStrength = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Attack(GenericUnit enemy)
    {
        Debug.Log("Aaaaattack!");
        enemy.GetHurt(this.damageStrength);
    }

    public override bool EnemiesInRange(GenericUnit enemy)
    {
        //define
        return true;
    }

    public override GenericUnit GetEnemiesInRange()
    {
        return null;
    }

    public override void GetHurt(int value)
    {
        Debug.Log("I'm hurt");
        this.health -= value;
        if(this.health < 0)
        {
            this.health = 0;
        }
    }

}
