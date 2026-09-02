using UnityEngine;

public class Slime : Enemy
{
    public bool canSplit = true; //False on the two children created by a split, so they cannot split again

    //Applies damage as normal, then checks whether this slime just crossed the 50% HP threshold
    //without dying outright. If so, it hands itself off to TurnManager to be replaced by two
    //smaller slimes that each inherit its exact current HP
    public override void TakeDamage(int damageTaken)
    {
        base.TakeDamage(damageTaken);

        if(canSplit == true && IsDead() == false && currentHP <= maxHP / 2)
        {
            canSplit = false; //Guards against this same instance triggering another split from further damage before the split finishes

            if(turnManager != null)
            {
                turnManager.HandleSlimeSplit(this);
            }
        }
    }
}
