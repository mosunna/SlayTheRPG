using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Enemy : Character
{
    public EnemyData sourceData; //Links back to the ScriptableObject

    public Intent CurrentIntent; //Set by ChooseNextIntent() during ENEMIES_CHOOSE_INTENT, read during ENEMY_TURN

    //private SpriteRenderer spriteRenderer;

    public UnityEngine.UI.Image hpBarFill;
    public UnityEngine.UI.Image hpBarGhostFill;
    public TMPro.TMP_Text hpNumberText;

    public UnityEngine.UI.Image intentIconImage; //Small icon shown above the enemy hinting at its next action

    //Decides this enemy's next action and stores it as CurrentIntent, to be read and executed during ENEMY_TURN
    public virtual void ChooseNextIntent(Character target, List<Enemy> allies) //Added allies to allow for LunaticCultist to target its ally
    {
        int rolledDamage = CombatActions.RollDamage(EffectiveAttack);

        int maxHits;

        if(sourceData != null)
        {
            maxHits = sourceData.maxHits;
        }
        else
        {
            maxHits = 1;
        }

        int hits = Random.Range(1, maxHits + 1);

        CurrentIntent = new Intent(IntentType.Attack, rolledDamage, hits, target, null);

    }

    //Shows the correct icon for this enemy's CurrentIntent, or hides it entirely for enemies whose
    //EnemyData has showsIntent set to false (the boss). Called by TurnManager right after ChooseNextIntent()
    public void UpdateIntentIcon(Sprite attackIcon, Sprite buffIcon)
    {
        if(intentIconImage == null)
        {
            return;
        }

        if(sourceData != null && sourceData.showsIntent == false)
        {
            intentIconImage.enabled = false;
            return;
        }

        if(CurrentIntent.type == IntentType.Attack)
        {
            intentIconImage.sprite = attackIcon;
            intentIconImage.enabled = true;
        }
        else if(CurrentIntent.type == IntentType.Buff)
        {
            intentIconImage.sprite = buffIcon;
            intentIconImage.enabled = true;
        }
        else
        {
            intentIconImage.enabled = false; //Charge/Defend/Expose currently only happen on the boss, which never shows an icon anyway
        }
    }

    //Executes the enemy's current intention, decisin will be applied through CombatActions
    public virtual void ExecuteIntent()
    {
        //If statements on the three possibiltiies of enemy intent (what they plan to do on their turn)

        if(CurrentIntent.type == IntentType.Attack)
        {
           int hits = Mathf.Max(CurrentIntent.hitCount, 1);

           for(int i = 0; i < hits; i++)
            {
                CombatActions.Attack(CurrentIntent.target, CurrentIntent.value);
            }
        }
        else if(CurrentIntent.type == IntentType.Defend)
        {
            CombatActions.Defend(this, CurrentIntent.value);
        }
        else if(CurrentIntent.type == IntentType.Buff)
        {
            //Where: if buff is suppose to be executed, it will have different branches depending on the character type casting it
        }
    }

    //A built in Unity method
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(sourceData != null)
        {
            InitializeFromSourceData();
        }
    }

    private void Update()
    {
        UpdateHPDisplay(hpBarFill, hpBarGhostFill, hpNumberText);
    }

    //Copies this enemy's stats from its EnemyData asset into the runtime fields (so i don't have to manually add in stats)
    public void InitializeFromSourceData()
    {
        CharacterName = sourceData.enemyName;
        maxHP = sourceData.maxHP;
        currentHP = sourceData.maxHP;
        attack = sourceData.attack;
        defense = sourceData.defense;
        spriteRenderer.sprite = sourceData.sprite;
    }

    private TurnManager turnManager;

    private void Start()
    {
        turnManager = FindAnyObjectByType<TurnManager>();
    }

    //A built in Unity method. Just activates when mouse clicked on in gameobjects
    private void OnMouseDown()
    {
        if(turnManager != null)
        {
            turnManager.SelectTarget(this);
        }
    }
}
