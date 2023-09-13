using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
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
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Kreeture kreeture) =>
                {
                    if  (Random.Range(2, 5) == 2)
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
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
