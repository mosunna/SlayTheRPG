using UnityEngine;

public enum IntentType
{
    Attack, //Deals damage to the target
    Defend, //Applies a temporary Defense bonus to self
    Buff //Increases another character's stats or own stats
}

public struct Intent
{
    public IntentType type; //What kind of action this intent represents
    public int value; //The damage, defense bonus, or buff amount tied to this intent
    public Character target; //Who this intent will act on when executed
    public Sprite icon; //The icon shown above the enemy during ENEMIES_CHOOSE_INTENT

    public Intent(IntentType type, int value, Character target, Sprite icon)
    {
        this.type = type;
        this.value = value;
        this.target = target;
        this.icon = icon;
    }
}
