using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private List<EncounterData> clearedEncounters = new List<EncounterData>();
    private bool bossRewardGranted = false;

    public string heroName = "Hero";
    public EncounterData selectedEncounter;
    public int requiredEncountersForBossReward = 3;

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

    private void Awake()
    {
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

        player.maxHP += 20; //Placeholder flat reward 
        player.currentHP += 20;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
