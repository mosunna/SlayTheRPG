using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public BattleState CurrentState; //The state the battle is currently in

    public Player player; //Assigned in the Inspector for now, until spawning exists
    public List<Enemy> enemies = new List<Enemy>(); //Assigned in the Inspector for now, until spawning exists

    public GameObject enemyPrefab;
    public Transform spawnPointLeft;
    public Transform spawnPointCenter;
    public Transform spawnPointRight;
    public EncounterData currentEncounter; //Hardcoded for now

    public SkillData healSkill;
    public SkillData chargeSkill;

    //Helper method to determine where the enemies should appear on screen depending on enemyCount in battle
    private List<Transform> GetSpawnFormation(int enemyCount)
    {
        List<Transform> points = new List<Transform>(); //The list storing the game space location of the enemies

        if(enemyCount == 1)
        {
            points.Add(spawnPointCenter);
        }
        else if(enemyCount == 2)
        {
            points.Add(spawnPointLeft);
            points.Add(spawnPointRight);
        }
        else if(enemyCount == 3)
        {
            points.Add(spawnPointLeft);
            points.Add(spawnPointCenter);
            points.Add(spawnPointRight);
        }

        return points;
    }

    //Player will select an option from the battle menu. Heal and Charge will be categorized under Spell
    private enum PlayerActionType
    {
        Attack, 
        Defend,
        Heal,
        Charge
    }
    
    private PlayerActionType pendingPlayerAction; // Which action was selected from UI buttons

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

    private bool awaitingTarget = false; //Whether or not the game is waiting for you to select a target
    //Called by the Attack button's OnClick()
    public void OnAttackButtonPressed()
    {
        if(CurrentState == BattleState.PLAYER_TURN)
        {
            pendingPlayerAction = PlayerActionType.Attack;
            awaitingTarget = true; //WAITS FOR PLAYER TO SELECT TARGET ENEMY
        }
    }

    //Called by the Defend button's OnClick()
    public void OnDefendButtonPressed()
    {
        if(CurrentState == BattleState.PLAYER_TURN)
        {
            pendingPlayerAction = PlayerActionType.Defend;
            awaitingTarget = false;
            SetState(BattleState.PLAYER_ACTION);
        }
    }

    //Called by the Heal button's OnClick(), inside the Spell submenu
    public void OnHealButtonPressed()
    {
        if(CurrentState == BattleState.PLAYER_TURN)
        {
            pendingPlayerAction = PlayerActionType.Heal;
            awaitingTarget = false;
            SetState(BattleState.PLAYER_ACTION);
        }
    }

    //Called by the Charge button's OnClick(), inside the Spell submenu
    public void OnChargeButtonPressed()
    {
        if(CurrentState == BattleState.PLAYER_TURN)
        {
            pendingPlayerAction = PlayerActionType.Charge;
            awaitingTarget = false;
            SetState(BattleState.PLAYER_ACTION);
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
        if(currentEncounter != null && enemyPrefab != null)
        {
            List<Transform> formation = GetSpawnFormation(currentEncounter.enemies.Count);
            for(int i = 0; i < currentEncounter.enemies.Count; i++)
            {
                if(i >= formation.Count)
                {
                    break;
                }

                GameObject newEnemyObject = Instantiate(enemyPrefab,formation[i].position, Quaternion.identity);
                Enemy newEnemy = newEnemyObject.GetComponent<Enemy>();

                if(newEnemy != null)
                {
                    newEnemy.sourceData = currentEncounter.enemies[i];
                    newEnemy.InitializeFromSourceData();
                    enemies.Add(newEnemy);
                }
            }
        }
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
            EnsureValidTarget();

            if(pendingPlayerAction == PlayerActionType.Attack)
            {
                int rolledDamage = CombatActions.RollDamage(player.EffectiveAttack);
                rolledDamage = player.ApplyChargeToDamage(rolledDamage);
                CombatActions.Attack(selectedTarget, rolledDamage); //NOW USES PLAYER SELECTED TARGETTING 
            }
            else if(pendingPlayerAction == PlayerActionType.Defend)
            {
                CombatActions.Defend(player, 3); //Placeholder bonus amount - tune once playtested
            }
            else if(pendingPlayerAction == PlayerActionType.Heal)
            {
                bool healResolved = CombatActions.Heal(player, healSkill);

                if(healResolved == false)
                {
                    Debug.Log("Not enough FP");
                    SetState(BattleState.PLAYER_TURN); //Not enough FP.. Let's you try again rather than taking your turn away
                    return;
                }
            }
            else if(pendingPlayerAction == PlayerActionType.Charge)
            {
                bool chargeResolved = CombatActions.Charge(player, chargeSkill);

                if(chargeResolved == false)
                {
                    SetState(BattleState.PLAYER_TURN);
                    return;
                }
            }
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

        //Decreases every enemy's buff by one turn asfter their action is processed
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].BuffDecay();
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

    private Enemy selectedTarget; //Which enemy is currently chosen as an attack target

    //Called by an Enemy's OnMouseDown() when the player clicks it
    public void SelectTarget(Enemy enemy)
    {
        if(enemy == null || enemy.IsDead())
        {
            return;
        }

        selectedTarget = enemy;

        if(awaitingTarget == true && CurrentState == BattleState.PLAYER_TURN)
        {
            awaitingTarget = false;
            SetState(BattleState.PLAYER_ACTION); //The click itself is what triggers the attack to resolve
        }
    }

    //Ensures selectedTarget always points at a valid, living enemy before it's used
    private void EnsureValidTarget()
    {
        if(selectedTarget != null && selectedTarget.IsDead() == false)
        {
            return;
        }

        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].IsDead() == false)
            {
                selectedTarget = enemies[i];
                return;
            }
        }
    }

    //Step past PLAYER_TURN from the Inspector to
    //test the full loop before BattleUI exists. REMOVE LATER
    [ContextMenu("Debug: Advance To Player Action")]
    private void DebugAdvanceToPlayerAction()
    {
        SetState(BattleState.PLAYER_ACTION);
    }
}
