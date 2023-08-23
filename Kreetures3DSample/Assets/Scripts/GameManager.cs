using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour{    
    public static GameManager Instance { get; private set; }// Singleton instance

    //Wild creature
    public Kreeture kreetureForBattle { get; set; }

    // Player-related data
    public List<Kreeture> playerTeam = new List<Kreeture>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the GameManager object when changing scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate GameManager instances
        }
    }

    public List<Kreeture> GetKreetureNames()
    {
        // Replace this with your actual logic to fetch Kreeture names
        // For example, you might have a list of Kreeture objects and you can extract names from them        
        List <Kreeture> kreetures = new List<Kreeture>();

        //Wild Kreeture is always going to be in the list first
        kreetures.Add(kreetureForBattle);

        //Player creature always should be second in the list
        kreetures.Add(playerTeam[0]);

        return kreetures;
    }    
}