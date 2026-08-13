using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public int maxHP;
    public int attack;
    public int defense;
    public bool showsIntent = true; //Boss will have this set to false 
}
