using UnityEngine;

public static class CombatActions
{
    //Deals damage to the target. The amount is passed in so it can come
    //from either a the player attacking or a previously chosen enemy attack
    public static void Attack(Character target, int damage)
    {
        target.TakeDamage(damage);  
    }

    //Spends FP and deals the skill's power as damage to the target. Returns false if
    //the caster doesn't have enough FP to use it
    public static bool UseSkill(Character caster, Character target, SkillData skill)
    {
        if (caster.currentFP < skill.fpCost)
        {
            return false;
        }

        caster.currentFP -= skill.fpCost;
        target.TakeDamage(skill.power);
        return true;
    }

    //Applies a temporary Defense bonus to whoever is defending
    public static void Defend(Character character, int bonusAmount)
    {
        character.Defend(bonusAmount);
    }

    public static int RollDamage(int baseAttack)
    {
        return Random.Range(baseAttack - 1, baseAttack + 2);
    }
}
