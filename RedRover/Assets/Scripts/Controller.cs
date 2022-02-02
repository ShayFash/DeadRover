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
            team1[0].Attack(team2[0]);
        }

    }

    void GetUnitsInRange(GenericUnit attacker)
    {
        if(team1.Contains(attacker)){
            foreach (GenericUnit unit in team2)
            {
                attacker.EnemiesInRange(unit);
            }
        }
        else
        {
            foreach (GenericUnit unit in team1)
            {
                //check
            }
        }
        
    }

}
