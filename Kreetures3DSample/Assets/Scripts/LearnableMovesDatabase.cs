using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LearnableMovesDatabase", menuName = "Kreeture/Learnable Moves Database")]
public class LearnableMovesDatabase : ScriptableObject
{
    public List<LearnableMove> learnableMoves;
}

[System.Serializable]
public class LearnableMove
{
    public Attack move;
    public int requiredLevel;
    public KreetureType[] compatibleTypes;
}