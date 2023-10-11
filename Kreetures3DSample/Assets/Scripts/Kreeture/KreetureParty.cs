using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KreetureParty : MonoBehaviour
{
    [SerializeField] public List<Kreeture> kreetures;

    public List<Kreeture> Kreetures
	{
		get
		{
            return kreetures;
		}
	}

    private void Start()
    {
        foreach (var kreeture in kreetures)
        {
            kreeture.Init();
        }
    }

    public Kreeture GetHealthyKreeture()
    {
        return kreetures.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddKreeture(Kreeture newKreeture)
    {
        if (kreetures.Count < 6)
        {
            kreetures.Add(newKreeture);
        }
        else
        {
            // TODO: Add to the PC once that's implemented
        }
    }
}
