using System;
using UnityEngine;

public class AI
{
    private readonly Controller controller;
    private readonly StateMachine stateMachine;

    public AI(Controller controller)
    {
        this.controller = controller;
        stateMachine = new StateMachine(controller);
    }
    public void PickUnit(GenericUnit[] units)
    {
        int index = UnityEngine.Random.Range(0, units.Length);
        GenericUnit unit = units[index];

        

        controller.SelectUnit(unit);
        Debug.Log("Picked unit");
    }

    public void DecideActions(GenericUnit actingUnit, GenericUnit[] units)
    {
        stateMachine.DecideActions(actingUnit, units);
    }

    private interface IState
    {
        public void DoActions(Controller controller);
    }

    private class Attack : IState
    {
        private GenericUnit targetUnit;

        public Attack(GenericUnit targetUnit)
        {
            this.targetUnit = targetUnit;
        }
        void IState.DoActions(Controller controller)
        {
            Debug.Log("Attacked unit");
            controller.Attack();
            controller.SelectUnit(targetUnit);
        }
    }

    private class Move : IState
    {
        private Vector3Int tilePosition;

        public Move(Vector3Int tilePosition)
        {
            this.tilePosition = tilePosition;
        }
        void IState.DoActions(Controller controller)
        {
            Debug.Log("Moved unit");
            controller.Move();
            controller.MoveSelectedUnit(tilePosition);
        }
    }

    private class StateMachine
    {
        IState state;
        Controller controller;

        public StateMachine(Controller controller)
        {
            this.controller = controller;
        }

        public void DecideActions(GenericUnit actingUnit, GenericUnit[] units)
        {
            state = Transition(actingUnit, units);
            state.DoActions(controller);

            if (state.GetType().Equals(typeof(Move)))
            {
                state = Transition(actingUnit, units);
                if (state.GetType().Equals(typeof(Attack)))
                {
                    state.DoActions(controller);
                } else
                {
                    controller.EndTurn();
                }
            }
        }

        private IState Transition(GenericUnit actingUnit, GenericUnit[] units)
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

            if (enemiesNearby.Length - alliesNearby.Length >= 2)
            {
                // Move away from enemy units
                int minEnemiesNearby = 1000;
                Vector3Int goodTilePosition = new Vector3Int(1000, 1000);
                foreach (Vector3Int tilePos in actingUnit.TilesInRange())
                {
                    if (!controller.HasTileAtPosition(tilePos))
                    {
                        continue;
                    }
                    if (goodTilePosition.Equals(new Vector3Int(1000, 1000)))
                    {
                        goodTilePosition = tilePos;
                    }
                    GenericUnit[] enemiesThatCanReach = Array.FindAll(enemies, delegate (GenericUnit enemy)
                    {
                        return enemy.TileInRange(tilePos);
                    });
                    if (enemiesThatCanReach.Length <= minEnemiesNearby)
                    {
                        minEnemiesNearby = enemiesThatCanReach.Length;
                        goodTilePosition = tilePos;
                    }
                }
                return new Move(goodTilePosition);
            }

            Array.Sort(enemies, delegate (GenericUnit unit1, GenericUnit unit2) {
                if (unit1.Health == unit2.Health)
                {
                    return 0;
                }
                else if (unit1.Health > unit2.Health)
                {
                    return 1;
                }
                else
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
                // Move towards enemy unit
                Vector3Int myTilePostion = actingUnit.GetTilePosition();

                int minTileDistance = 100000;
                GenericUnit closestEnemy = enemies[0]; // If this crashes at some point, fix it
                for (int i = 0; i < enemies.Length; i++)
                {
                    Vector3Int theirTilePosition = enemies[i].GetTilePosition();

                    int tileDistance = 0;
                    for (int d = 0; d <= 1; d++)
                    {
                        tileDistance += Math.Abs(myTilePostion[d] - theirTilePosition[d]);

                        if (tileDistance < minTileDistance)
                        {
                            minTileDistance = tileDistance;
                            closestEnemy = enemies[i];
                        }
                    }
                }

                Vector3Int goodTilePosition = new Vector3Int(1000, 1000);
                foreach (Vector3Int tilePos in actingUnit.TilesInRange())
                {
                    if (!controller.HasTileAtPosition(tilePos))
                    {
                        continue;
                    }
                    if (goodTilePosition.Equals(new Vector3Int(1000, 1000)))
                    {
                        goodTilePosition = tilePos;
                    }

                    int tileDistance = 0;
                    for (int d = 0; d <= 1; d++)
                    {
                        tileDistance += Math.Abs(tilePos[d] - closestEnemy.GetTilePosition()[d]);

                        if (tileDistance <= minTileDistance)
                        {
                            minTileDistance = tileDistance;
                            goodTilePosition = tilePos;
                        }
                    }
                }

                return new Move(goodTilePosition);
            }

            return new Attack(enemies[index]);
        }
    }
} 
