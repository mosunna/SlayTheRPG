using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string CharacterName;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int maxFP;
    public int currentFP;
    
    private int bonusDefense = 0; //The bonus defense a character gains when using block on their turn

    //Checks if character is dead
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    //Applying damage taken to character
    public virtual void TakeDamage(int damageTaken)
    {
        int totalDefense = defense + bonusDefense;
        int mitigatedDmg = Mathf.Max(damageTaken - defense, 1); //The character will always take at least 1 damage
        currentHP = Mathf.Max(currentHP - mitigatedDmg, 0); //Prevents health from ever going below 0
    }

    //Applying health restore to character
    public virtual void Heal(int healthResored)
    {
        currentHP = Mathf.Min(healthResored + healthResored, maxHP);
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

}
/*
public class Player : Character
{
    
}

public class Enemy : Character
{
    
}
*/
