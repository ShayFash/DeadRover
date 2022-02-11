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
            Debug.Log("There are " + team1[0].GetEnemiesInRange(team2).Count + " enemies are in range.");
        }

    }

}
