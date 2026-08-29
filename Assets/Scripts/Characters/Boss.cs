using UnityEngine;
using System.Collections.Generic;

public class Boss : Enemy
{
    private int cycleTurn = 0; //Tracks position in the fixed 4-turn cycle: 1=Charges, 2=Attacks, 3=Shields, 4=Exposed

    public override void ChooseNextIntent(Character target, List<Enemy> allies) //Added allies to allow for LunaticCultist to target its ally
    {
        cycleTurn++;
        if(cycleTurn > 4)
        {
            cycleTurn = 1;
        }

        //CurrentIntent isn't shown to the player (EnemyData.showsIntent is false for the boss),
        //but ExecuteIntent() still needs a stored target for its Attack turn
        CurrentIntent = new Intent(IntentType.Attack, 0, 1, target, null);
    }

    public override void ExecuteIntent()
    {
        if(cycleTurn == 1)
        {
            Debug.Log("[Boss] Turn 1: Charging"); //TEMP
            //Charges: recovers from being exposed, then powers up this cycle's Attack turn
            damageMultiplier = 1f;
            ApplyCharge();

            if(spriteRenderer != null)
            {
                spriteRenderer.sprite = sourceData.sprite; //Reverses from exposed sprite to normal
            }
        }
        else if(cycleTurn == 2)
        {
            //Attacks: a charged hit if Turn 1 successfully set it up
            int rolledDamage = CombatActions.RollDamage(EffectiveAttack);
            rolledDamage = ApplyChargeToDamage(rolledDamage);
            CombatActions.Attack(CurrentIntent.target, rolledDamage);
        }
        else if(cycleTurn == 3)
        {
            //Shields: bonus defense for this cycle
            Debug.Log("Lavos curls in its shell!");
            CombatActions.Defend(this, 5); //Placeholder bonus amount
        }
        else if(cycleTurn == 4)
        {
           
            //Exposes core: stays vulnerable through the player's following turn,
            //until Turn 1 of the next cycle resets it
            damageMultiplier = 2f; //Double damage received

            if(spriteRenderer != null)
            {
                spriteRenderer.sprite = sourceData.exposedSprite;
            }
        }
    }
}