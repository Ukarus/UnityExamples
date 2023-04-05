using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tuxemon
{
    public string name;
    public float baseHP;
    public float currentHP;
    public float attack;
    public float defense;
    public float speed;
    public string spritePath;

    public Tuxemon(string name, float baseHP, float currentHP, float attack, float defense, float speed, string spritePath)
    { 
        this.name = name;
        this.baseHP = baseHP;
        this.currentHP = currentHP;
        this.attack = attack;
        this.defense = defense;
        this.speed = speed;
        this.spritePath = spritePath;
    }

    public void TakeDamage(float opponentAttack)
    {
        var damage = Mathf.Max(1, opponentAttack - defense);
        currentHP -= damage;
    }
}

public class Turn
{
    public Tuxemon attacker;
    public Tuxemon defender;
    public Slider attackerBar;
    public Slider defenderBar;
    public bool isAttackFinished = false;
    public bool isTurnFinished = false;

    public Turn(Tuxemon attacker, Tuxemon defender, Slider attackerBar, Slider defenderBar)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.attackerBar = attackerBar;
        this.defenderBar = defenderBar;
    }
}

public class BattleState : MonoBehaviour
{
    public Slider playerBar;
    public Slider opponentBar;
    public TMPro.TextMeshProUGUI battleText;
    public float duration = 0.5f;
    public AudioClip hitSound;
    public Image playerImage;
    public Image opponentImage;

    private Tuxemon playerTuxemon;
    private Tuxemon opponentTuxemon;
    private AudioSource audioSource;
    private float gameDoneTimer = 0f;

    enum FightState
    {
        Waiting,
        Fighting,
        Done
    }
    private FightState fState = FightState.Waiting;
    private Stack<Turn> turns;

    void Start()
    {
        
        playerTuxemon = new Tuxemon(
                "Aardt",
                50,
                50,
                9,
                5,
                4,
                "Graphics/tuxemon/aardart-back"
        );
        opponentTuxemon = new Tuxemon(
                "Agnite",
                45,
                45,
                7,
                6,
                5,
                "Graphics/tuxemon/agnite-front"
       );

        turns = new Stack<Turn>();

        // Set the initial values for the hp bars
        playerBar.maxValue = playerTuxemon.baseHP;
        opponentBar.maxValue = opponentTuxemon.baseHP;
        playerBar.value = playerTuxemon.currentHP;
        opponentBar.value = opponentTuxemon.currentHP;

        audioSource = GetComponent<AudioSource>();

        Debug.Log(Resources.Load(playerTuxemon.spritePath));
        playerImage.sprite = Resources.Load<Sprite>(playerTuxemon.spritePath);
        opponentImage.sprite = Resources.Load<Sprite>(opponentTuxemon.spritePath);
    }

    // Update is called once per frame
    void Update()
    {
        if (fState == FightState.Fighting)
        {

            if (turns.Count < 1)
            {
                fState = FightState.Waiting;
                battleText.text = "Choose an action";
                return;
            }

            var turn = turns.Peek();
            var oldHp = turn.defender.currentHP;
            if (!turn.isAttackFinished)
            {
                battleText.text = $"{turn.attacker.name} attacks {turn.defender.name}";
                turn.defender.TakeDamage(turn.attacker.attack);

                turn.isAttackFinished = true;
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(hitSound);
                }
            }
            else
            {
                StartCoroutine(AnimateAttack(oldHp, turn.defender.currentHP, turn));
            }

            if (turn.isTurnFinished)
            {
                turns.Pop();
            }
            IsGameDone();
        } 
        else if (fState == FightState.Done)
        {
            gameDoneTimer += Time.deltaTime;

            if (gameDoneTimer > 3f)
            {
                OnRunAction();
            }

        }

    }

    public void IsGameDone()
    {
        // Check if player or enemy tuxemon has fainted
        if (playerTuxemon.currentHP <= 0)
        {
            battleText.text = "You fainted";
            fState = FightState.Done;
        }
        else if (opponentTuxemon.currentHP <= 0)
        {

            battleText.text = "You win";
            fState = FightState.Done;
        }
    }

    private IEnumerator AnimateAttack(float startValue, float endValue, Turn turn)
    {
        float startTime = Time.time;
        float endTime = startTime + duration;
        

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration; // calculate the current progress of the animation (0 to 1)
            turn.defenderBar.value = Mathf.Lerp(startValue, endValue, t); // interpolate between the start and end positions
            yield return null;
        }

        // set the final position to ensure accuracy
        turn.defenderBar.value = endValue;
        turn.isTurnFinished = true;
    } 
   

    public void OnFightAction()
    {

        if (fState != FightState.Fighting)
        {
            fState = FightState.Fighting;
            Turn turn1;
            Turn turn2;
            // if player tuxemon is faster than opponent
            if (playerTuxemon.speed > opponentTuxemon.speed)
            {
                turn1 = new Turn(playerTuxemon, opponentTuxemon, playerBar, opponentBar);
                turn2 = new Turn(opponentTuxemon, playerTuxemon, opponentBar, playerBar);
            } 
            else
            {
                turn1 = new Turn(opponentTuxemon, playerTuxemon, opponentBar, playerBar);
                turn2 = new Turn(playerTuxemon, opponentTuxemon, playerBar, opponentBar);
            }

            // we take turns from the top of the stack, so the first turn must go at the end
            turns.Push(turn2);
            turns.Push(turn1);
        }
    }

    public void OnRunAction()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
