using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int fpCost; 
    public int power;
    public Sprite icon;
}
