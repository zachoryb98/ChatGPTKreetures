using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Kreeture> wildKreetures;

    public Kreeture GetRandomWildPokemon()
    {
        var wildPokemon = wildKreetures[Random.Range(0, wildKreetures.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }
}