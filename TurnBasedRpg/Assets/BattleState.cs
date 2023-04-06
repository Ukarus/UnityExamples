using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class Tuxemon
{
    public string name;
    public float baseHP;
    public float currentHP;
    public float attack;
    public float defense;
    public float speed;
    public string spriteFrontPath;
    public string spriteBackPath;

    public Tuxemon(string name, float baseHP, float currentHP, float attack, float defense, float speed, string spriteFrontPath, string spriteBackPath)
    { 
        this.name = name;
        this.baseHP = baseHP;
        this.attack = attack;
        this.defense = defense;
        this.speed = speed;
        this.spriteFrontPath = spriteFrontPath;
        this.spriteBackPath = spriteBackPath;
        this.currentHP = currentHP;
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

[System.Serializable]
public class Tuxemons
{
    //employees is case sensitive and must match the string "employees" in the JSON.
    public Tuxemon[] tuxemons;
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
    public Tuxemons tuxemons;
    public GameObject playerPanel;
    public GameObject opponentPanel;
    public AudioSource victoryMusic;
    public AudioSource battleMusic;

    private Tuxemon playerTuxemon;
    private Tuxemon opponentTuxemon;
    private AudioSource audioSource;
    private float gameDoneTimer = 0f;
    private bool isWaitingForFainted = false;


    enum FightState
    {
        Waiting,
        Fighting,
        Done
    }
    private FightState fState = FightState.Waiting;
    private Stack<Turn> turns;

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }

    


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerTuxemon = tuxemons.tuxemons[0];
        opponentTuxemon = tuxemons.tuxemons[1];

        turns = new Stack<Turn>();


        SetupTuxemonUI(playerPanel, playerTuxemon);
        SetupTuxemonUI(opponentPanel, opponentTuxemon);


        playerImage.sprite = Resources.Load<Sprite>(playerTuxemon.spriteBackPath);
        opponentImage.sprite = Resources.Load<Sprite>(opponentTuxemon.spriteFrontPath);

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
                IsGameDone();
            }
        } 
        else if (fState == FightState.Done)
        {
            gameDoneTimer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                OnRunAction();
            }

            //if (gameDoneTimer > 3f && !isWaitingForFainted)
            //{
            //    OnRunAction();
            //}

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
            opponentPanel.GetComponent<Animator>().SetBool("IsOpponentFainted", true);
            isWaitingForFainted = true;
            victoryMusic.Play();
            battleMusic.Stop();
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

        if (fState == FightState.Waiting)
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

    private void SetupTuxemonUI(GameObject panel, Tuxemon tuxemon)
    {
        panel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = tuxemon.name;
        Slider slider = panel.transform.GetChild(1).GetComponent<Slider>();
        slider.maxValue = tuxemon.baseHP;
        slider.value = tuxemon.currentHP;
    }
}
