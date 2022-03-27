using System;
using System.Collections;
using UnityEngine;

public class AI
{
    private readonly Controller controller;
    private readonly StateMachine stateMachine;
    
    static float Delay = 1;

    public AI(Controller controller)
    {
        this.controller = controller;
        stateMachine = new StateMachine(controller);
    }
    public IEnumerator PickUnit(GenericUnit[] units)
    {
        yield return new WaitForSeconds(Delay * 2);

        int index = UnityEngine.Random.Range(0, units.Length);
        GenericUnit unit = units[index];

        controller.SelectUnit(unit);
    }

    public void DecideActions(GenericUnit actingUnit, GenericUnit[] units)
    {
        controller.StartCoroutine(stateMachine.DecideActions(actingUnit, units));
    }

    private interface IState
    {
        IEnumerator DoActions(Controller controller);
    }

    private class Attack : IState
    {
        private GenericUnit targetUnit;

        public Attack(GenericUnit targetUnit)
        {
            this.targetUnit = targetUnit;
        }
        IEnumerator IState.DoActions(Controller controller)
        {
            yield return new WaitForSeconds(Delay);
            controller.Attack();

            yield return new WaitForSeconds(Delay);
            controller.SelectUnit(targetUnit);
        }
    }

    private class Move : IState
    {
        private Vector3 tilePosition;

        public Move(Vector3 tilePosition)
        {
            this.tilePosition = tilePosition;
        }
        IEnumerator IState.DoActions(Controller controller)
        {
            yield return new WaitForSeconds(Delay);
            controller.Move();

            yield return new WaitForSeconds(Delay);
            controller.TryMoveSelectedUnit(tilePosition);
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

        public IEnumerator DecideActions(GenericUnit actingUnit, GenericUnit[] units)
        {
            state = Transition(actingUnit, units);

            // StartCoroutine is only for MonoBehaviours, so use the controller to start it
            yield return controller.StartCoroutine(state.DoActions(controller));

            if (state.GetType().Equals(typeof(Move)))
            {
                state = Transition(actingUnit, units);
                if (state.GetType().Equals(typeof(Attack)))
                {
                    yield return controller.StartCoroutine(state.DoActions(controller));
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
                Vector3 goodTilePosition = Vector3.positiveInfinity;
                foreach (Tile tile in actingUnit.TilesInRange())
                {
                    Vector3 tilePos = tile.transform.position;
                    if (!controller.HasTileAtPosition(tilePos))
                    {
                        continue;
                    }
                    if (goodTilePosition.Equals(Vector3.positiveInfinity))
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
                Vector3 myTilePostion = actingUnit.GetTilePosition();

                float minTileDistance = Mathf.Infinity;
                GenericUnit closestEnemy = enemies[0]; // If this crashes at some point, fix it
                for (int i = 0; i < enemies.Length; i++)
                {
                    Vector3 theirTilePosition = enemies[i].GetTilePosition();

                    float tileDistance = 0;
                    tileDistance += Mathf.Abs(myTilePostion[0] - theirTilePosition[0]);
                    tileDistance += Mathf.Abs(myTilePostion[2] - theirTilePosition[2]);

                    if (tileDistance < minTileDistance)
                    {
                        minTileDistance = tileDistance;
                        closestEnemy = enemies[i];
                    }
                }

                Vector3 goodTilePosition = Vector3.positiveInfinity;
                foreach (Tile tile in actingUnit.TilesInRange())
                {
                    Vector3 tilePos = tile.transform.position;
                    if (!controller.HasTileAtPosition(tilePos) || controller.TileOccupied(tilePos))
                    {
                        continue;
                    }
                    if (goodTilePosition.Equals(Vector3.positiveInfinity))
                    {
                        goodTilePosition = tilePos;
                    }

                    float tileDistance = 0;
                    tileDistance += Mathf.Abs(tilePos[0] - closestEnemy.GetTilePosition()[0]);
                    tileDistance += Mathf.Abs(tilePos[2] - closestEnemy.GetTilePosition()[2]);

                    if (0 < tileDistance && tileDistance <= minTileDistance)
                    {
                        minTileDistance = tileDistance;
                        goodTilePosition = tilePos;
                    }
                }

                return new Move(goodTilePosition);
            }

            return new Attack(enemies[index]);
        }
    }
} 
