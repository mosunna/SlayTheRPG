using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public BattleState CurrentState; //The state the battle is currently in

    public Player player; //Assigned in the Inspector for now, until spawning exists
    public List<Enemy> enemies = new List<Enemy>(); //Assigned in the Inspector for now, until spawning exists

    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public GameObject cultistPrefab;

    GameObject prefabToSpawn; //Unknown and only assigned if it's the final boss fight or not
    public Transform spawnPointLeft;
    public Transform spawnPointCenter;
    public Transform spawnPointRight;
    public Transform bossSpawnPoint;
    public EncounterData currentEncounter; //Hardcoded for now

    public GameObject turnBannerPanel; //Small banner box shown briefly for "Player Turn" / "Enemy Turn"
    public TMP_Text turnText; //Text inside turnBannerPanel

    public GameObject actionLogPanel; //Banner box below turnBannerPanel, recaps what just happened this turn
    public TMP_Text actionLogText;
    public GameObject victoryPanel;
    public TMP_Text victoryText;
    public GameObject continueButton;

    public GameObject defeatPanel;
    public TMP_Text defeatText;
    public GameObject restartButton;
    public GameObject quitButton;

    public AudioSource audioSource;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
    public AudioClip bossVictoryMusic;

    private const float EnemyTurnAnnounceDelay = 0.75f; //Pause after "Enemy Turn" shows before the first enemy acts
    private const float PostBattleBannerDelay = 1.5f; //Pause after the banner appears before its buttons show (at the end of battle)
    private const float EnemyActionStaggerDelay = 0.4f; //Pause between each enemy's action when there's more than one
    private const float EnemyTurnResultDelay = 0.5f; //Pause after the last enemy acts before checking win/lose
    private const float TurnBannerDisplayDuration = 1f; //How long the Player Turn banner stays visible before hiding
    private const float ActionLogDisplayDuration = 1.5f; //How long the action log banner stays visible before hiding
    private const float BossEndingPauseDelay = 1f;

    public SkillData healSkill;
    public SkillData chargeSkill;

    private GameManager gameManager;
    private Coroutine actionLogHideRoutine; //Tracks the pending hide so a new action always gets the full display duration

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

    //Shows the small Player/Enemy Turn banner
    private void ShowTurnBanner(string message)
    {
        if(turnBannerPanel != null)
        {
            turnBannerPanel.SetActive(true);
        }

        if(turnText != null)
        {
            turnText.text = message;
        }
    }

    //Hides the small Player/Enemy Turn banner
    private void HideTurnBanner()
    {
        if(turnBannerPanel != null)
        {
            turnBannerPanel.SetActive(false);
        }
    }

    //Shows the action log banner, describing what just happened this turn
    private void ShowActionLog(string message)
    {
        if(actionLogPanel != null)
        {
            actionLogPanel.SetActive(true);
        }

        if(actionLogText != null)
        {
            actionLogText.text = message;
        }

        if(actionLogHideRoutine != null)
        {
            StopCoroutine(actionLogHideRoutine);
        }

        actionLogHideRoutine = StartCoroutine(HideActionLogAfterDelay());
    }

    //Waits, then hides the action log banner. Restarted by every new ShowActionLog call
    //so a fresh message always gets the full display duration
    private IEnumerator HideActionLogAfterDelay()
    {
        yield return new WaitForSeconds(ActionLogDisplayDuration);

        if(actionLogPanel != null)
        {
            actionLogPanel.SetActive(false);
        }
    }

    //Builds and shows the action log text for an enemy's just executed intent
    private void LogEnemyAction(Enemy enemy)
    {
        if(enemy == null)
        {
            return;
        }

        if(enemy.CurrentIntent.type == IntentType.Attack)
        {
            //Boss's Attack turn always lands the charged hit from the previous turn, so it gets a heavier line
            //to cue the player toward blocking, regular enemies keep the plain attack line
            Boss bossEnemy = enemy as Boss;
            if(bossEnemy != null)
            {
                ShowActionLog($"{enemy.CharacterName} unleashes a crushing blow!");
            }
            else
            {
                ShowActionLog($"{enemy.CharacterName} attacks!");
            }
        }
        else if(enemy.CurrentIntent.type == IntentType.Defend)
        {
            ShowActionLog($"{enemy.CharacterName} braces itself. It's straining to hold firm!");
        }
        else if(enemy.CurrentIntent.type == IntentType.Buff)
        {
            string targetName = "an ally";
            if(enemy.CurrentIntent.target != null)
            {
                targetName = enemy.CurrentIntent.target.CharacterName;
            }

            ShowActionLog($"{enemy.CharacterName} buffs {targetName}!");
        }
        else if(enemy.CurrentIntent.type == IntentType.Charge)
        {
            ShowActionLog($"{enemy.CharacterName} is charging up!");
        }
        else if(enemy.CurrentIntent.type == IntentType.Expose)
        {
            ShowActionLog($"{enemy.CharacterName} is exhausted. Core exposed!");
        }
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
        gameManager = FindAnyObjectByType<GameManager>();
        
        if(gameManager != null && gameManager.selectedEncounter != null)
        {
            currentEncounter = gameManager.selectedEncounter;
        }

        if(gameManager != null)
        {
            gameManager.LoadPlayerStats(player);
        }

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
            StartCoroutine(HandlePlayerTurnRoutine());
        }
        else if (newState == BattleState.PLAYER_ACTION)
        {
            HandlePlayerAction();
        }
        else if (newState == BattleState.ENEMY_TURN)
        {
            StartCoroutine(HandleEnemyTurn());
        }
        else if (newState == BattleState.CHECK_WIN_LOSE)
        {
            HandleCheckWinLose();
        }
        else if (newState == BattleState.VICTORY)
        {
            if(IsBossEncounter() == true)
            {
                StartCoroutine(HandleBossEndingRoutine());
            }
            else
            {
                StartCoroutine(HandleVictoryRoutine());
            }
        }
        else if (newState == BattleState.DEFEAT)
        {
            StartCoroutine(HandleDefeatRoutine());
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

    //Initalizes and spawns enemies for the current encounter
    private void HandleSpawnEnemies()
    {
        enemies.Clear(); //To prevent any previous loaded enemies from being spawned into the scene
        if(currentEncounter != null && enemyPrefab != null)
        {
            List<Transform> formation = GetSpawnFormation(currentEncounter.enemies.Count);
            for(int i = 0; i < currentEncounter.enemies.Count; i++)
            {
                if(i >= formation.Count)
                {
                    break;
                }

                EnemyData dataToSpawn = currentEncounter.enemies[i];

                //The final boss uses it's own special prefab since it is more than just a regular enemy. 
                GameObject prefabToSpawn;
                Vector3 spawnPosition;
                if(dataToSpawn.isBoss == true)
                {
                    prefabToSpawn = bossPrefab;
                    spawnPosition = bossSpawnPoint.position;
                }
                else if(dataToSpawn.isLunaticCultist == true)
                {
                    prefabToSpawn = cultistPrefab;
                    spawnPosition = formation[i].position;
                }
                else
                {
                    prefabToSpawn = enemyPrefab;
                    spawnPosition = formation[i].position;
                }

                GameObject newEnemyObject = Instantiate(prefabToSpawn,spawnPosition, Quaternion.identity);
                Enemy newEnemy = newEnemyObject.GetComponent<Enemy>();

                if(newEnemy != null)
                {
                    newEnemy.sourceData = currentEncounter.enemies[i];
                    newEnemy.InitializeFromSourceData();
                    enemies.Add(newEnemy);
                }
            }
        }
        
        if(currentEncounter != null && currentEncounter.enemyActsFirst == true)
        {
            StartCoroutine(HandleAmbushRoutine());
        }
        else
        {
            SetState(BattleState.ENEMIES_CHOOSE_INTENT);
        }
    }

    //Plays out a one-time ambush hit before the normal turn loop begins, for the final boss
    private IEnumerator HandleAmbushRoutine()
    {
        Debug.Log("Lavos caught you off guard!"); //TEMP debug message
        ShowTurnBanner("Lavos caught you off guard!");
        yield return new WaitForSeconds(EnemyTurnAnnounceDelay);
        HideTurnBanner();

        if(player != null)
        {
            CombatActions.IgnoredDefenseAttack(player, 5); //Flat, unavoidable ambush damage, kept small since it is a surprise beat, not a real threat
        }

        yield return new WaitForSeconds(EnemyTurnResultDelay);
        SetState(BattleState.CHECK_WIN_LOSE);
    }

    //Calls ChooseNextIntent() on each enemy so their action is decided before the player acts
    private void HandleEnemiesChooseIntent()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].ChooseNextIntent(player, enemies);
        }

        SetState(BattleState.PLAYER_TURN);
    }

    //Resets the player's Defend bonus at the start of their turn, then waits for input
    private IEnumerator HandlePlayerTurnRoutine()
    {
        if(player != null) //Preventing code not compiling in Unity error
        {
            player.ResetDefense();
        }

        ShowTurnBanner("Player Turn");
        yield return new WaitForSeconds(TurnBannerDisplayDuration);
        HideTurnBanner();

        //TODO: BattleUI input here
    }

    //Handles player's chosen actins and then moves to enemy's turn
    private void HandlePlayerAction()
    {
        string heroName = "Hero";
        if(gameManager != null && gameManager.heroName != "")
        {
            heroName = gameManager.heroName;
        }

        if(player != null && enemies.Count > 0) //player null check set for debugged PLAYER_ACTION
        {
            EnsureValidTarget();

            if(pendingPlayerAction == PlayerActionType.Attack)
            {
                int rolledDamage = CombatActions.RollDamage(player.EffectiveAttack);
                rolledDamage = player.ApplyChargeToDamage(rolledDamage);
                CombatActions.Attack(selectedTarget, rolledDamage); //NOW USES PLAYER SELECTED TARGETTING 

                if(selectedTarget is Boss)
                {
                    CombatActions.Attack(player,3); //Thorns like mechanic dealt to the player when attacking the boss
                }

                ShowActionLog($"{heroName} attacks!");
            }
            else if(pendingPlayerAction == PlayerActionType.Defend)
            {
                CombatActions.Defend(player, 3); //Placeholder bonus amount - tune once playtested
                ShowActionLog($"{heroName} defends!");
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

                ShowActionLog($"{heroName} heals!");
            }
            else if(pendingPlayerAction == PlayerActionType.Charge)
            {
                bool chargeResolved = CombatActions.Charge(player, chargeSkill);

                if(chargeResolved == false)
                {
                    SetState(BattleState.PLAYER_TURN);
                    return;
                }

                ShowActionLog($"{heroName} charges!");
            }
        }
        SetState(BattleState.ENEMY_TURN);
    }

    //Resets each enemy's Defend bonus at the start of its own turn
    private IEnumerator HandleEnemyTurn()
    {
        ShowTurnBanner("Enemy Turn");
        yield return new WaitForSeconds(EnemyTurnAnnounceDelay);
        HideTurnBanner();
        //Resets every enemy at once through the loop
        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].IsDead() == true)
            {
                continue; //Fixes bug that has dead enemies still acting out intents
            }

            enemies[i].ResetDefense();
            enemies[i].ExecuteIntent();

            LogEnemyAction(enemies[i]);

            yield return new WaitForSeconds(EnemyActionStaggerDelay);
        }
            Debug.Log("[TurnManager] Enemy loop complete"); //TEMP

        //Decreases every enemy's buff by one turn asfter their action is processed
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].BuffDecay();
        }

        yield return new WaitForSeconds(EnemyTurnResultDelay);
        SetState(BattleState.CHECK_WIN_LOSE);
    }

    //Handles the check for whether the player or all enemies are dead
    //Returns true if the encounter currently being fought includes the final boss
    private bool IsBossEncounter()
    {
        if(currentEncounter == null || currentEncounter.enemies == null)
        {
            return false;
        }

        for(int i = 0; i < currentEncounter.enemies.Count; i++)
        {
            if(currentEncounter.enemies[i].isBoss == true)
            {
                return true;
            }
        }

        return false;
    }

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
            if(gameManager != null)
            {
                gameManager.RegisterEncounterCleared(currentEncounter, player);
                gameManager.ApplyPostEncounterRecovery(player);
            }
            SetState(BattleState.VICTORY);
            return;
        }
        
        SetState(BattleState.ENEMIES_CHOOSE_INTENT);
    }

    //Shows the victory banner, names the defeated enemy (or "X and friends" for multi-enemy fights),
//plays victory music, then reveals the Continue button after a short delay
private IEnumerator HandleVictoryRoutine()
{
    if(victoryPanel != null)
    {
        victoryPanel.SetActive(true);
    }

    string enemyName = "The enemies";
    if(enemies.Count > 0)
    {
        enemyName = enemies[0].CharacterName;
        if(enemies.Count > 1)
        {
            enemyName += " and friends";
        }
    }

    if(victoryText != null)
    {
        victoryText.text = $"{enemyName} has been defeated!";
    }

    if(audioSource != null && victoryMusic != null)
    {
        audioSource.clip = victoryMusic;
        audioSource.Play();
    }

    if(continueButton != null)
    {
        continueButton.SetActive(false);
    }

    yield return new WaitForSeconds(PostBattleBannerDelay);

    if(continueButton != null)
    {
        continueButton.SetActive(true);
    }
}

//Plays a music cue, waits briefly, then hands off to the Main Menu scene to show the ending screen
private IEnumerator HandleBossEndingRoutine()
{
    if(audioSource != null && bossVictoryMusic != null)
    {
        audioSource.clip = bossVictoryMusic;
        audioSource.Play();
    }

    yield return new WaitForSeconds(BossEndingPauseDelay);

    string heroName = "Hero";
    if(gameManager != null && gameManager.heroName != "")
    {
        heroName = gameManager.heroName;
    }

    string bossName = "The boss";
    if(enemies.Count > 0)
    {
        bossName = enemies[0].CharacterName;
    }

    if(gameManager != null)
    {
        gameManager.endingMessage = $"{heroName} has defeated {bossName}!";
        gameManager.showEndingScreen = true;
    }

    SceneManager.LoadScene("Main Menu");
}

//Shows the defeat banner with the hero's name, then reveals Restart/Quit after a short delay
private IEnumerator HandleDefeatRoutine()
{
    if(defeatPanel != null)
    {
        defeatPanel.SetActive(true);
    }

    string heroName = "Hero";
    if(gameManager != null && gameManager.heroName != "")
    {
        heroName = gameManager.heroName;
    }

    if(defeatText != null)
    {
        defeatText.text = $"{heroName} has been defeated!";
    }

    if(restartButton != null)
    {
        restartButton.SetActive(false);
    }

    if(quitButton != null)
    {
        quitButton.SetActive(false);
    }

    yield return new WaitForSeconds(PostBattleBannerDelay);

    if(restartButton != null)
    {
        restartButton.SetActive(true);
    }

    if(quitButton != null)
    {
        quitButton.SetActive(true);
    }
}

    //Called by the Continue button's OnClick() after victory. Returns to Choose Level, not Title
    public void OnContinuePressed()
    {
        if(gameManager != null)
        {
            gameManager.skipToLevelSelect = true;
        }

        SceneManager.LoadScene("Main Menu");
    }

    //Called by the Restart button's OnClick() after defeat. Reloads the same encounter fresh
    public void OnRestartPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    //Called by the Quit button's OnClick() after defeat. Returns to the Title screen
    public void OnQuitPressed()
    {
        SceneManager.LoadScene("Main Menu");
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
