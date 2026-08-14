using UnityEngine;

public class Enemy : Character
{
    public EnemyData sourceData; //Links back to the ScriptableObject

    public Intent CurrentIntent; //Set by ChooseNextIntent() during ENEMIES_CHOOSE_INTENT, read during ENEMY_TURN

    //Decides this enemy's next action and stores it as CurrentIntent, to be read and executed during ENEMY_TURN
    public void ChooseNextIntent(Character target)
    {
        //TODO: branch on enemy type once more than an acttack only behavior exists
        //TODO: assign a real icon once the IntentIcon system exists in Phase 3
        CurrentIntent = new Intent(IntentType.Attack, attack, target, null);
    }

    //Executes the enemy's current intention, decisin will be applied through CombatActions
    public void ExecuteIntent()
    {
        //If statements on the three possibiltiies of enemy intent (what they plan to do on their turn)
        //Where if buff is suppose to be executed, it will have different branches depending on the character type casting it
    }
}
