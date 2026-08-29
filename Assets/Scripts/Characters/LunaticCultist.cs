using System.Collections.Generic;
using UnityEngine;

public class LunaticCultist : Enemy
{
    private const int BuffAmount = 3; //Attack bonus granted per cast... MIGHT BE ADJUSTED IF OP

    public override void ChooseNextIntent(Character target, List<Enemy> allies)
    {
        Enemy allyToBuff = FindAlly(allies);

        if(allyToBuff != null)
        {
            //Assign a real icon once the IntentIcon system exists in later edits
            CurrentIntent = new Intent(IntentType.Buff, BuffAmount, 1, allyToBuff, null);
        }
        else
        {
            int rolledDamage = CombatActions.RollDamage(EffectiveAttack);
            CurrentIntent = new Intent(IntentType.Attack, rolledDamage, 1, target, null);
        }
    }

    public override void ExecuteIntent()
    {
        if(CurrentIntent.type == IntentType.Buff)
        {
            CurrentIntent.target.ApplyBuff(CurrentIntent.value);
            Debug.Log($"[LunaticCultist] Buffed {CurrentIntent.target.CharacterName} for {CurrentIntent.value} attack"); //TEMP
        }
        else if(CurrentIntent.type == IntentType.Attack)
        {
            CombatActions.Attack(CurrentIntent.target, CurrentIntent.value);
            Debug.Log("[LunaticCultist] No living ally. Attacking instead"); //TEMP
        }
    }

    //Finds the first living enemy in the list that isn't this cultist itself
    private Enemy FindAlly(List<Enemy> allies)
    {
        for(int i = 0; i < allies.Count; i++)
        {
            if(allies[i] != this && allies[i].IsDead() == false)
            {
                return allies[i];
            }
        }

        return null;
    }
}