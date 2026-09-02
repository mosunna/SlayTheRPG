using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public Sprite exposedSprite; //Used by Boss during its Exposed turn 
    public int maxHP;
    public int attack;
    public int defense;
    public int maxHits = 1; //Max number of hits the specific enemy can attack, it will roll for a number up to this int.
    public bool showsIntent = true; //Boss will have this set to false
    public bool isBoss = false; //Will only be true to for TurnManager to spawn the boss prefab
    public bool isLunaticCultist = false; //Will only be true for TurnManager to spawn the LunaticCultist prefab
    public bool isSlime = false; //Will only be true for TurnManager to spawn the SlimePrefab

    public float defendChance = 0f; //Chance (0-1) this enemy chooses Defend instead of Attack on its turn
    public int defendBonus = 3; //Bonus defense applied when this enemy defends
}
