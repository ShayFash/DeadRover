using UnityEngine;

public class AI
{
    private readonly Controller controller;

    public AI(Controller c)
    {
        controller = c;
    }
    public void PickUnit(GenericUnit[] units)
    {
        int index = Random.Range(0, units.Length);
        GenericUnit unit = units[index];

        controller.SelectUnit(unit);
    }
}
