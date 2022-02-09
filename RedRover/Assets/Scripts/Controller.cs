using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Option: Use tags instead?!
    public List<GenericUnit> team1 = new List<GenericUnit>();
    public List<GenericUnit> team2 = new List<GenericUnit>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed.");
            List<GenericUnit> enemiesInRange = team1[0].GetEnemiesInRange(team2);
            foreach (GenericUnit enemy in enemiesInRange)
            {
                team1[0].Attack(enemy);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key was pressed.");
            GetUnitsInRange(team1[0]);
        }

    }

    void GetUnitsInRange(GenericUnit attacker)
    {
        List<GenericUnit> enemiesInRange = new List<GenericUnit>();
        if(team1.Contains(attacker)){
            enemiesInRange = attacker.GetEnemiesInRange(team2);
            Debug.Log(enemiesInRange.Count);
        }
        else
        {
            enemiesInRange = attacker.GetEnemiesInRange(team1);
            Debug.Log(enemiesInRange.Count);
        }
        
    }

}
