using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private List<EncounterData> clearedEncounters = new List<EncounterData>();
    private bool bossRewardGranted = false;
    public bool skipToLevelSelect = false; //For when the player hits continue after a victory. Takes them back to the level select

    public string heroName = "Hero";
    public EncounterData selectedEncounter;
    public int requiredEncountersForBossReward = 3;

    private const float HPRecoveryPercent = 0.3f; //Restores a portion of missing HP after each win, softens the no-healing attrition run without fully undoing it. Placeholder - tune once playtested
    private const float FPRecoveryPercent = 0.6f; //Higher than HP's rate since FP's pool is much smaller, a flat 30% barely moves it. Keeps Charge usable across multiple fights. Placeholder - tune once playtested

    private bool hasSavedPlayerStats = false;
    private int savedMaxHP;
    private int savedCurrentHP;
    private int savedMaxFP;
    private int savedCurrentFP;

    public void RegisterEncounterCleared(EncounterData encounter, Player player)
    {
        if(encounter == null || bossRewardGranted == true)
        {
            return;
        }

        if(clearedEncounters.Contains(encounter) == false)
        {
            clearedEncounters.Add(encounter);
        }

        if(clearedEncounters.Count >= requiredEncountersForBossReward)
        {
            bossRewardGranted = true;
            ApplyBossPrepReward(player);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Awake() //Edited so that the game loop can properly happen when a player finishes a level. Without this, the game always returns back to title screen rather than level select
    {
        GameManager[] existingManagers = FindObjectsByType<GameManager>();

        if(existingManagers.Length > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    
    //Called by TurnManager when a battle is won. Tracks which encounters have been cleared, and grants
    //a one-time stat boost once enough of them have been beaten, in preparation for the boss fight
    private void ApplyBossPrepReward(Player player)
    {
        if(player == null)
        {
            return;
        }

        //The buff that the player receives as a hidden rewards for completing all levels prior to the final boss
        player.currentHP += 20;
        player.currentFP += 5;

        //Checks to ensure player stats don't go over their max with the flat addition buffs
        if(player.currentHP > player.maxHP)
        {
            player.currentHP = player.maxHP;
        }

        
        if(player.currentFP > player.maxFP)
        {
            player.currentFP = player.maxFP;
        }
    }

    //Called by TurnManager after every win. Restores a percentage of missing HP/FP,
    //separate from the one-time ApplyBossPrepReward above. Keeps the run's no-healing
    //tension while preventing it from fully draining the player before the boss fight
    public void ApplyPostEncounterRecovery(Player player)
    {
        if(player == null)
        {
            return;
        }

        int missingHP = player.maxHP - player.currentHP;
        player.currentHP += Mathf.RoundToInt(missingHP * HPRecoveryPercent);

        if(player.currentHP > player.maxHP)
        {
            player.currentHP = player.maxHP;
        }

        int missingFP = player.maxFP - player.currentFP;
        player.currentFP += Mathf.RoundToInt(missingFP * FPRecoveryPercent);

        if(player.currentFP > player.maxFP)
        {
            player.currentFP = player.maxFP;
        }

        SavePlayerStats(player);
    }

    //Copies the player's current stats onto GameManager so they survive the next scene load
    public void SavePlayerStats(Player player)
    {
        if(player == null)
        {
            return;
        }

        hasSavedPlayerStats = true;
        savedMaxHP = player.maxHP;
        savedCurrentHP = player.currentHP;
        savedMaxFP = player.maxFP;
        savedCurrentFP = player.currentFP;
    }

    //Restores the player's saved stats once a new battle scene loads. Does nothing on the very
    //first battle of a run, since nothing is saved yet and the Inspector defaults should be used
    public void LoadPlayerStats(Player player)
    {
        if(player == null || hasSavedPlayerStats == false)
        {
            return;
        }

        player.maxHP = savedMaxHP;
        player.currentHP = savedCurrentHP;
        player.maxFP = savedMaxFP;
        player.currentFP = savedCurrentFP;
    }

    //Clears all carried-over run state. Called when starting a brand new run from the Title screen,
    //so Quit -> Play doesn't inherit HP/FP or cleared encounters from the previous attempt
    public void ResetRun()
    {
        clearedEncounters.Clear();
        bossRewardGranted = false;
        hasSavedPlayerStats = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
