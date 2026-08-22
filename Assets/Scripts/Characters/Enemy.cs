using UnityEngine;

public class Enemy : Character
{
    public EnemyData sourceData; //Links back to the ScriptableObject

    public Intent CurrentIntent; //Set by ChooseNextIntent() during ENEMIES_CHOOSE_INTENT, read during ENEMY_TURN

    private SpriteRenderer spriteRenderer;

    //Decides this enemy's next action and stores it as CurrentIntent, to be read and executed during ENEMY_TURN
    public void ChooseNextIntent(Character target)
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

        //TODO: assign a real icon once the IntentIcon system exists in Phase 3
        CurrentIntent = new Intent(IntentType.Attack, rolledDamage, hits, target, null);

    }

    //Executes the enemy's current intention, decisin will be applied through CombatActions
    public void ExecuteIntent()
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
            InitalizeFromSourceData();
        }
    }

    //Copies this enemy's stats from its EnemyData asset into the runtime fields (so i don't have to manually add in stats)
    private void InitalizeFromSourceData()
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
