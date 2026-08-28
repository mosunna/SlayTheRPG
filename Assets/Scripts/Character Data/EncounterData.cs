using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounter", menuName = "Encounter Data")]
public class EncounterData : ScriptableObject
{
    public List<EnemyData> enemies; //Which enemy will be present in fight
    public bool enemyActsFirst = false; //Only true for the boss
}
