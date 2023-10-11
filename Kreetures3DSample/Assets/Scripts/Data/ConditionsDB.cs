using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Id = ConditionID.psn,
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Kreeture kreeture) =>
                {
                    kreeture.UpdateHP(kreeture.MaxHp / 8);
                    kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Id = ConditionID.brn,
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Kreeture kreeture) =>
                {
                    kreeture.UpdateHP(kreeture.MaxHp / 16);
                    kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Id = ConditionID.par,
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Kreeture kreeture) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name}'s paralyzed and can't move");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Id = ConditionID.frz,
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Kreeture kreeture) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        kreeture.CureStatus();
                        kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name}'s is not frozen anymore");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Id = ConditionID.slp,
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Kreeture kreeture) =>
                {
                    // Sleep for 1-3 turns
                    kreeture.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {kreeture.StatusTime} moves");
                },
                OnBeforeMove = (Kreeture kreeture) =>
                {
                    if (kreeture.StatusTime <= 0)
                    {
                        kreeture.CureStatus();
                        kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} woke up!");
                        return true;
                    }

                    kreeture.StatusTime--;
                    kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} is sleeping");
                    return false;
                }
            }
        },

        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Id = ConditionID.confusion,
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Kreeture kreeture) =>
                {
                    // Confused for 1 - 4 turns
                    kreeture.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {kreeture.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Kreeture kreeture) =>
                {
                    if (kreeture.VolatileStatusTime <= 0)
                    {
                        kreeture.CureVolatileStatus();
                        kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    kreeture.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Hurt by confusion
                    kreeture.StatusChanges.Enqueue($"{kreeture.Base.Name} is confused");
                    kreeture.UpdateHP(kreeture.MaxHp / 8);
                    kreeture.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
	{
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
	}
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
