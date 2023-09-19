using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Kreeture> wildKreetures;

    public Kreeture GetRandomWildKreeture()
    {
        var wildKreeture = wildKreetures[Random.Range(0, wildKreetures.Count)];
        wildKreeture.Init();
        return wildKreeture;
    }
}