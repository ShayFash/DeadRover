using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : GenericUnit
{

    // Start is called before the first frame update
    void Start()
    {
        base.health = 100;
        base.attack = 1;
        base.reach = 2;
    }

}