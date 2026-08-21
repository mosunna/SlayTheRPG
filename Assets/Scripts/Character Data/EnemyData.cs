using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public int maxHP;
    public int attack;
    public int defense;
    public int maxHits = 1; //Max number of hits the specific enemy can attack, it will roll for a number up to this int.
    public bool showsIntent = true; //Boss will have this set to false 
}
