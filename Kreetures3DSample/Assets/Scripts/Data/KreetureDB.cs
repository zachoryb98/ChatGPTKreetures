using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KreetureDB
{
    static Dictionary<string, KreetureBase> kreetures;

    public static void Init()
    {
        kreetures = new Dictionary<string, KreetureBase>();

        var kreetureArray = Resources.LoadAll<KreetureBase>("Kreetures");
        foreach (var kreeture in kreetureArray)
        {
            if (kreetures.ContainsKey(kreeture.Name))
            {
                Debug.LogError($"There are two Kreetures with the name {kreeture.Name}");
                continue;
            }

            kreetures[kreeture.Name] = kreeture;
        }
    }

    public static KreetureBase GetKreetureByName(string name)
    {
        if (!kreetures.ContainsKey(name))
        {
            Debug.LogError($"Kreeture with name {name} not found in the database");
            return null;
        }

        return kreetures[name];
    }
}