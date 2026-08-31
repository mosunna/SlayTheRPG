using System;
using System.Collections;
using System.Reflection;
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

    public float damageMultiplier = 1f; //Stat strictly for the final boss' special state
    private int bonusDefense = 0; //The bonus defense a character gains when using block on their turn
    private int bonusAttack = 0; //Temporary attack bonus from an attack buff being cast
    private int buffTurnsRemaining = 0; //Turn duration left on current buff before it is removed from character
    private bool isCharged = false; //Only true if player selects the charge spell
    private const int ChargedDamageMultiplier = 2;

    private const float HPBarTweenDuration = 0.8f; //How long the main bar takes to animate to a new HP value

    private float displayedHP; //The HP value the main bar is currently showing/animating toward
    private bool displayedHPInitialized = false;
    private Coroutine hpBarCoroutine;
    private int lastObservedHP; //The currentHP value the tween last reacted to, so mid-tween frames don't restart it

    private const float FPBarTweenDuration = 0.8f; //How long the FP bar takes to animate to a new FP value

    private float displayedFP; //The FP value the main bar is currently showing/animating toward
    private bool displayedFPInitialized = false;
    private Coroutine fpBarCoroutine;
    private int lastObservedFP; //The currentFP value the tween last reacted to, so mid-tween frames don't restart it

    public SpriteRenderer spriteRenderer;
    private const float DamageFlashDuration = 0.3f;
    private const int DamageFlashBlinkCount = 3;
    private Coroutine damageFlashCoroutine;

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
        mitigatedDmg = Mathf.RoundToInt(mitigatedDmg * damageMultiplier);
        currentHP = Mathf.Max(currentHP - mitigatedDmg, 0); //Prevents health from ever going below 0

        PlayDamageFlash();
    }

    //Method strictly used for final boss. Used to ignore player defense and deal 5 HP regardless
    public virtual void IgnoredDefenseDamage(int damageTaken)
    {
        int finalDamage = Mathf.RoundToInt(damageTaken * damageMultiplier);
        currentHP = Mathf.Max(currentHP - finalDamage, 0);

        PlayDamageFlash();
    }

    private void PlayDamageFlash()
    {
        if(spriteRenderer == null) //Currently player is a null sprite, so this fixes null error
        {
            return;
        }

        damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float blinkDuration = DamageFlashDuration / (DamageFlashBlinkCount * 2);

        for(int i =0; i < DamageFlashBlinkCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkDuration);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkDuration);
        }
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

    //Marks player as charged
    public virtual void ApplyCharge()
    {
        isCharged = true;
    }

    //Doubles the given damage if charged, then clears the charge by setting back to false. Returns damage unchanged otherwise
    public int ApplyChargeToDamage(int damage)
    {
        if(isCharged == false)
        {
            return damage;
        }

        isCharged = false;
        return damage * ChargedDamageMultiplier;
}

    //Call this every frame (from a subclass's Update()) to drive a main HP bar, a trailing "ghost" bar,
    //and an HP number text. mainBarFill animates smoothly toward currentHP. ghostBarFill stays frozen at
    //the pre-damage value for the duration of the animation, then snaps down to match once it finishes.
    //Any of the three parameters can be left null if that piece of UI doesn't exist for this character.
    protected void UpdateHPDisplay(UnityEngine.UI.Image mainBarFill, UnityEngine.UI.Image ghostBarFill, TMPro.TMP_Text hpNumberText)
    {
        if(displayedHPInitialized == false)
        {
            displayedHP = currentHP;
            lastObservedHP = currentHP;
            displayedHPInitialized = true;

            if(ghostBarFill != null)
            {
                ghostBarFill.fillAmount = (float)currentHP / Mathf.Max(maxHP, 1);
            }
        }

        //Only react when currentHP actually changes (a new hit/heal), not every frame a tween is still mid-flight -
        //otherwise this would restart the coroutine every frame and it would never reach its final step
        if(currentHP != lastObservedHP)
        {
            lastObservedHP = currentHP;

            if(hpBarCoroutine != null)
            {
                StopCoroutine(hpBarCoroutine);
            }

            hpBarCoroutine = StartCoroutine(AnimateHPBar(mainBarFill, ghostBarFill));
        }

        if(mainBarFill != null)
        {
            mainBarFill.fillAmount = displayedHP / Mathf.Max(maxHP, 1);
        }

        if(hpNumberText != null)
        {
            hpNumberText.text = $"HP: {currentHP}/{maxHP}";
        }
    }

    //Animates displayedHP from its current value toward currentHP over HPBarTweenDuration seconds.
    //ghostBarFill is left untouched (still showing the old value) until the animation finishes, then snaps to match.
    private IEnumerator AnimateHPBar(UnityEngine.UI.Image mainBarFill, UnityEngine.UI.Image ghostBarFill)
    {
        float startValue = displayedHP;
        float endValue = currentHP;
        float elapsed = 0f;

        while(elapsed < HPBarTweenDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / HPBarTweenDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); //Cubic ease-out: fast at first, slows into a crawl near the end
            displayedHP = Mathf.Lerp(startValue, endValue, easedT);

            if(mainBarFill != null)
            {
                mainBarFill.fillAmount = displayedHP / Mathf.Max(maxHP, 1);
            }

            yield return null;
        }

        displayedHP = endValue;

        if(mainBarFill != null)
        {
            mainBarFill.fillAmount = displayedHP / Mathf.Max(maxHP, 1);
        }

        if(ghostBarFill != null)
        {
            ghostBarFill.fillAmount = (float)currentHP / Mathf.Max(maxHP, 1);
        }
    }

    //Call this every frame (from a subclass's Update()) to drive a main FP bar, a trailing "ghost" bar,
    //and an FP number text. Mirrors UpdateHPDisplay exactly, just tracking FP instead of HP.
    //Any of the three parameters can be left null if that piece of UI doesn't exist for this character.
    protected void UpdateFPDisplay(UnityEngine.UI.Image mainBarFill, UnityEngine.UI.Image ghostBarFill, TMPro.TMP_Text fpNumberText)
    {
        if(displayedFPInitialized == false)
        {
            displayedFP = currentFP;
            lastObservedFP = currentFP;
            displayedFPInitialized = true;

            if(ghostBarFill != null)
            {
                ghostBarFill.fillAmount = (float)currentFP / Mathf.Max(maxFP, 1);
            }
        }

        //Only react when currentFP actually changes, not every frame a tween is still mid-flight -
        //otherwise this would restart the coroutine every frame and it would never reach its final step
        if(currentFP != lastObservedFP)
        {
            lastObservedFP = currentFP;

            if(fpBarCoroutine != null)
            {
                StopCoroutine(fpBarCoroutine);
            }

            fpBarCoroutine = StartCoroutine(AnimateFPBar(mainBarFill, ghostBarFill));
        }

        if(mainBarFill != null)
        {
            mainBarFill.fillAmount = displayedFP / Mathf.Max(maxFP, 1);
        }

        if(fpNumberText != null)
        {
            fpNumberText.text = $"FP: {currentFP}/{maxFP}";
        }
    }

    //Animates displayedFP from its current value toward currentFP over FPBarTweenDuration seconds.
    //ghostBarFill is left untouched (still showing the old value) until the animation finishes, then snaps to match.
    private IEnumerator AnimateFPBar(UnityEngine.UI.Image mainBarFill, UnityEngine.UI.Image ghostBarFill)
    {
        float startValue = displayedFP;
        float endValue = currentFP;
        float elapsed = 0f;

        while(elapsed < FPBarTweenDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / FPBarTweenDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); //Cubic ease-out: fast at first, slows into a crawl near the end
            displayedFP = Mathf.Lerp(startValue, endValue, easedT);

            if(mainBarFill != null)
            {
                mainBarFill.fillAmount = displayedFP / Mathf.Max(maxFP, 1);
            }

            yield return null;
        }

        displayedFP = endValue;

        if(mainBarFill != null)
        {
            mainBarFill.fillAmount = displayedFP / Mathf.Max(maxFP, 1);
        }

        if(ghostBarFill != null)
        {
            ghostBarFill.fillAmount = (float)currentFP / Mathf.Max(maxFP, 1);
        }
    }

}
