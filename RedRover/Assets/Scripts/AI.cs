using System;

public class AI
{
    private readonly Controller controller;

    public AI(Controller controller)
    {
        this.controller = controller;
    }
    public void PickUnit(GenericUnit[] units)
    {
        int index = UnityEngine.Random.Range(0, units.Length);
        GenericUnit unit = units[index];

        controller.SelectUnit(unit);
    }

    public void DecideActions(GenericUnit actingUnit, GenericUnit[] units)
    {

    }

    private interface IState
    {
        public IState Transition(GenericUnit actingUnit, GenericUnit[] units);
        public void DoActions(Controller controller);
    }

    private class Attack : IState
    {
        private GenericUnit actingUnit;
        private GenericUnit targetUnit;

        private Attack(GenericUnit actingUnit, GenericUnit targetUnit)
        {
            this.actingUnit = actingUnit;
            this.targetUnit = targetUnit;
        }
        void IState.DoActions(Controller controller)
        {
            controller.Attack();
            controller.SelectUnit(targetUnit);
        }

        IState IState.Transition(GenericUnit actingUnit, GenericUnit[] units)
        {
            GenericUnit[] enemies = Array.FindAll(units, delegate (GenericUnit u)
            {
                return !actingUnit.CompareTag(u.tag) && !u.SwitchingSides;
            });

            GenericUnit[] allies = Array.FindAll(units, delegate (GenericUnit u)
            {
                return actingUnit.CompareTag(u.tag) && !u.SwitchingSides;
            });

            GenericUnit[] enemiesNearby = Array.FindAll(enemies, delegate (GenericUnit enemy)
            {
                return enemy.UnitInRange(actingUnit) || enemy.TileInRange(actingUnit.GetTilePosition());
            });

            GenericUnit[] alliesNearby = Array.FindAll(allies, delegate (GenericUnit ally)
            {
                bool nearbyEnemyInRange = Array.Exists(enemiesNearby, delegate (GenericUnit enemy) {
                    return ally.UnitInRange(enemy) || ally.TileInRange(enemy.GetTilePosition());
                });

                bool nearToActingUnit = ally.TileInRange(actingUnit.GetTilePosition());

                return nearbyEnemyInRange || nearToActingUnit;
            });

            if (enemiesNearby.Length - alliesNearby.Length > 2)
            {
                return new Move();
            }

            Array.Sort(enemies, delegate (GenericUnit unit1, GenericUnit unit2) {
                if (unit1.Health == unit2.Health)
                {
                    return 0;
                } else if (unit1.Health > unit2.Health)
                {
                    return 1;
                } else
                {
                    return 0;
                }
            });

            int index = Array.FindIndex(enemies, delegate (GenericUnit enemy)
            {
                return actingUnit.UnitInRange(enemy);
            });

            if (index == -1)
            {
                return new Move();
            }

            return new Attack(actingUnit, enemies[index]);
        }
    }

    private class Move : IState
    {
        void IState.DoActions(Controller controller)
        {
            throw new System.NotImplementedException();
        }

        IState IState.Transition(GenericUnit actingUnit, GenericUnit[] units)
        {
            throw new System.NotImplementedException();
        }
    }

    private class StateMachine
    {
        IState state;
        Controller controller;

        private StateMachine(Controller c)
        {
            controller = c;
        }
    }
} 
