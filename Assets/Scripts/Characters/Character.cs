using System;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public string CharacterName;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int maxFP;
    public int currentFP;
    
    private int bonusDefense = 0; //The bonus defense a character gains when using block on their turn
    private int bonusAttack = 0; //Temporary attack bonus from an attack buff being cast
    private int buffTurnsRemaining = 0; //Turn duration left on current buff before it is removed from character

    public int EffectiveAttack
    {
        get {return attack + bonusAttack;}
    }

    //Checks if character is dead
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    //Applying damage taken to character
    public virtual void TakeDamage(int damageTaken)
    {
        int totalDefense = defense + bonusDefense;
        int mitigatedDmg = Mathf.Max(damageTaken - totalDefense, 1); //The character will always take at least 1 damage
        currentHP = Mathf.Max(currentHP - mitigatedDmg, 0); //Prevents health from ever going below 0
    }

    //Applying health restore to character
    public virtual void Heal(int healthResored)
    {
        currentHP = Mathf.Min(currentHP + healthResored, maxHP);
    }

    //Applying defense to character
    public virtual void Defend(int bonusAmount)
    {
        bonusDefense += bonusAmount;
    }

    //Resetting bonus defense 
    public virtual void ResetDefense()
    {
        bonusDefense = 0;
    }

    //Applies a buff to character. Stacks onto existing duration (if any)
    public virtual void ApplyBuff(int bonusAmount)
    {
        bonusAttack = bonusAmount;
        buffTurnsRemaining  += 2;
    }

    //Ages the buff by one turn phase, and removing it once its duration is over
    public virtual void BuffDecay()
    {
        if(buffTurnsRemaining <= 0)
        {
            return;
        }

        buffTurnsRemaining--;

        if(buffTurnsRemaining <= 0)
        {
            bonusAttack = 0;
        }
    }

}
/*
public class Player : Character
{
    
}

public class Enemy : Character
{
    
}
*/
