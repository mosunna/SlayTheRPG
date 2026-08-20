using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public BattleState CurrentState; //The state the battle is currently in

    public Player player; //Assigned in the Inspector for now, until spawning exists
    public List<Enemy> enemies = new List<Enemy>(); //Assigned in the Inspector for now, until spawning exists

    private void Start()
    {
        SetState(BattleState.START);
    }

    //Transitions the battle to a new state and logs it
    public void SetState(BattleState newState)
    {
        CurrentState = newState;
        Debug.Log($"[TurnManager] Entering state: {newState}");

        if (newState == BattleState.START)
        {
            HandleStart();
        }
        else if (newState == BattleState.SPAWN_ENEMIES)
        {
            HandleSpawnEnemies();
        }
        else if (newState == BattleState.ENEMIES_CHOOSE_INTENT)
        {
            HandleEnemiesChooseIntent();
        }
        else if (newState == BattleState.PLAYER_TURN)
        {
            HandlePlayerTurn();
        }
        else if (newState == BattleState.PLAYER_ACTION)
        {
            HandlePlayerAction();
        }
        else if (newState == BattleState.ENEMY_TURN)
        {
            HandleEnemyTurn();
        }
        else if (newState == BattleState.CHECK_WIN_LOSE)
        {
            HandleCheckWinLose();
        }
    }

    //TODO: the actual battle setup
    private void HandleStart()
    {
        SetState(BattleState.SPAWN_ENEMIES);
    }

    //TODO: Spawn enemies for this encounter
    private void HandleSpawnEnemies()
    {
        SetState(BattleState.ENEMIES_CHOOSE_INTENT);
    }

    //Calls ChooseNextIntent() on each enemy so their action is decided before the player acts
    private void HandleEnemiesChooseIntent()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].ChooseNextIntent(player);
        }

        SetState(BattleState.PLAYER_TURN);
    }

    //Resets the player's Defend bonus at the start of their turn, then waits for input
    private void HandlePlayerTurn()
    {
        if(player != null) //Preventing code not compiling in Unity error
        {
            player.ResetDefense();
        }

        //TODO: BattleUI input here
    }

    //Handles player's chosen actins and then moves to enemy's turn
    private void HandlePlayerAction()
    {
        if(player != null && enemies.Count > 0) //player null check set for debugged PLAYER_ACTION
        {
            CombatActions.Attack(enemies[0], player.attack); //Attack enemies[0] as a test
        }
        SetState(BattleState.ENEMY_TURN);
    }

    //Resets each enemy's Defend bonus at the start of its own turn
    private void HandleEnemyTurn()
    {
        //Resets every enemy at once through the loop
        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].IsDead() == true)
            {
                continue; //Fixes bug that has dead enemies still acting out intents
            }

            enemies[i].ResetDefense();
            enemies[i].ExecuteIntent();
        }

        SetState(BattleState.CHECK_WIN_LOSE);
    }

    //Handles the check for whether the player or all enemies are dead
    private void HandleCheckWinLose()
    {
        if(player != null && player.IsDead())
        {
            SetState(BattleState.DEFEAT);
            return;
        }

        bool allEnemiesDead = true;

        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].IsDead() == false)
            {
                allEnemiesDead = false;
                break;
                
            }

        }

        if(allEnemiesDead && enemies.Count > 0)
        {
            SetState(BattleState.VICTORY);
            return;
        }
        
        SetState(BattleState.ENEMIES_CHOOSE_INTENT);
    }

    //Step past PLAYER_TURN from the Inspector to
    //test the full loop before BattleUI exists. REMOVE LATER
    [ContextMenu("Debug: Advance To Player Action")]
    private void DebugAdvanceToPlayerAction()
    {
        SetState(BattleState.PLAYER_ACTION);
    }
}
