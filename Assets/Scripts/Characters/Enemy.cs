using UnityEngine;

public class Enemy : Character
{
    public EnemyData sourceData; //Links back to the ScriptableObject

    public Intent CurrentIntent; //Set by ChooseNextIntent() during ENEMIES_CHOOSE_INTENT, read during ENEMY_TURN

    //Decides this enemy's next action and stores it as CurrentIntent, to be read and executed during ENEMY_TURN
    public void ChooseNextIntent(Character target)
    {
        //TODO: branch on enemy type once more than Slime's attack-only behavior exists
        //TODO: assign a real icon once the IntentIcon system exists in Phase 3
        CurrentIntent = new Intent(IntentType.Attack, attack, target, null);
    }
}
