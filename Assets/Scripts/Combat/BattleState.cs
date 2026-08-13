public enum BattleState
{
    START, //Battle begins, before enemies are spawned
    SPAWN_ENEMIES, //Enemies for this encounter are instantiated
    ENEMIES_CHOOSE_INTENT, //Each enemy picks its next action via ChooseNextIntent()
    PLAYER_TURN, //Waiting for the player to choose an action
    PLAYER_ACTION, //Player's chosen action resolves (damage, defend, skill)
    ENEMY_TURN, //Each enemy executes its previously chosen intent
    CHECK_WIN_LOSE //Checks whether the battle has been won or lost; loops back to ENEMIES_CHOOSE_INTENT if the battle continues
}
