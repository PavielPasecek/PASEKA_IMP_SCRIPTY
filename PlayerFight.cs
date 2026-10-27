using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFight : MonoBehaviour
{
    public GameObject CharacterModel; // model člena týmu
    public GameObject MomentalniFight; // souboj, do kterého patří

    // nastavení proměnných pro životy, energii, bránění a status
    public int MaxStamina;
    public int Stamina;
    public int Level;
    public int MaxHP;
    public int CurrentHP;
    private int SmoothStamina;
    private int SmoothHp;
    public bool Guard = false;
    public StatusEfekt aktualniStatus = StatusEfekt.Zadny;

    // proměnné pro grafiku nad postavou
    public GameObject canvas;
    private Slider HPBar;
    private Slider StaminaBar;
    public GameObject SelectIndicator;
    private TMP_Text HealthTextFull;
    private TMP_Text HealthTextCurrent;
    private TMP_Text StaminaTextFull;
    private TMP_Text StaminaTextCurrent;

    // proměnné pro efekty ublížení, nebo efekty útočení

    public TMP_Text DamageEffect;
    float EffectTimer = 0f;
    Vector3 EffectStart;

    public List<GameObject> Efekty;
    float lookAtTimer = 0f;
    ParticleSystem lookAtParticle;
    Transform lookAtTarget;

    Vector3 selIndPos;

    private AttackHandler FightAttackHandler;

    // indikátor postavy
    public enum Postavy { Maki, Amma, Reno, Gruno }
    public Postavy Postava;

    private Animator animator;

    private void Awake()
    {
        animator = CharacterModel.GetComponent<Animator>(); // nastaví hráčovi základní animace
        animator.SetLayerWeight(animator.GetLayerIndex("Fight"), 1);
        animator.SetFloat("Speed", 0f);
    }
    public void StartPlayer(AttackHandler a) // spustí se při začátku souboje
    {
        FightAttackHandler = a;
        Stamina = MaxStamina;
        CurrentHP = MaxHP;
        SmoothStamina = Stamina;
        SmoothHp = CurrentHP;
        DamageEffect.enabled = false;
        EffectStart = DamageEffect.gameObject.transform.position;
        selIndPos = SelectIndicator.transform.position;
        SetGUI();
    }

    public void SetPlayerUI(GameObject HP, GameObject MHP, GameObject HPSlider, GameObject S, GameObject MS, GameObject SSlider)
    {
        HPBar = HPSlider.GetComponent<Slider>();
        StaminaBar = SSlider.GetComponent<Slider>();
        HealthTextCurrent = HP.GetComponent<TMP_Text>();
        HealthTextFull= MHP.GetComponent<TMP_Text>();
        StaminaTextCurrent = S.GetComponent<TMP_Text>();
        StaminaTextFull = MS.GetComponent<TMP_Text>();
        SetGUI();
    }
    public void TakeDamage(int attackindex, int lvl,StatusEfekt StatusEffect)
    {
        float sanceNaNetrefu = StatusEffect == StatusEfekt.Slepota ? 1f : 0;
        if (Random.value + sanceNaNetrefu > FightAttackHandler.GetPresnost(attackindex)) // možnost vyhnutí, pokud je protivníkův útok nepřesný
        {
            if (FightAttackHandler.GetDamage(attackindex) > 0)
            {
                Efekt(Color.white, "MIMO", 0.5f);
            }
            return;
        }
        if (Random.value + sanceNaNetrefu > FightAttackHandler.GetStatusChance(attackindex)) // možnost vyhnutí se statusu, pokud útok neubírá životy a status se nepovedl
        {
            if (FightAttackHandler.GetDamage(attackindex) == 0)
            {
                Efekt(Color.white, "MIMO", 0.5f);
                return;
            }
        }
        else
        {
            aktualniStatus = FightAttackHandler.GetStatus(attackindex); // nastavení statusu
        }
        float multi = Guard ? 0.7f : 1; // pokud se brání, dostane pouze 70% poškození
        int damage = FightAttackHandler.GetDamage(attackindex);
        int vypocet = Mathf.FloorToInt((damage + (damage * (lvl * 0.2f))) * multi); // výpočet poškození
        CurrentHP -= vypocet;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP); //ubrání životů
        SetGUI();
        if (CurrentHP <= 0) // pokud byl hráč poražen
        {
            Efekt(Color.white, "KO", 0f);
            Defeat();

        }
        else
        {
            if (damage > 0) // pokud nebyl poražen
            {
                Efekt(Color.red, vypocet.ToString().TrimStart('-'), 0f); // ukáže efekt ubrání životů
                if (!FightAttackHandler.GetIsForMoreEnemies(attackindex))
                {
                    Invoke("HitAnimace", 0.5f);
                }
                else
                {
                    HitAnimace();
                }
            }
            else if (damage != 0 && damage <= 0) // pokud útok neléčí, nepřehraje se animace ublížení, pouze efekt zeleného léčení
            {
                Efekt(Color.green, vypocet.ToString().TrimStart('-'), 0f);
            }
        }
    }
    private void LateUpdate()
    {
        if (Camera.main)
        {
            canvas.transform.LookAt(canvas.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up); // otočí grafiku směrem ke kameře
        }
        CharacterModel.transform.position = transform.position + Vector3.down;
        CharacterModel.transform.rotation = new Quaternion(0,0,0,0); // otočí hráče správným směrem
        if (SmoothHp != CurrentHP) // plynulé ubrání životů
        {
            if (CurrentHP > SmoothHp)
            {
                SmoothHp++;
            }
            else
            {
                SmoothHp--;
            }
            SetGUI();
        }
        if (SmoothStamina != Stamina) // plynulé ubrání staminy
        {
            if (Stamina > SmoothStamina)
            {
                SmoothStamina++;
            }
            else
            {
                SmoothStamina--;
            }
            SetGUI();
        }
        Effects(); // výpočet efektů
        SelectIndicator.transform.position = selIndPos + Vector3.up * Mathf.Sin(Time.time * 2) * 0.03f; // animace indikátoru vybární
        animator.SetLayerWeight(animator.GetLayerIndex("Fight"), 1);
    }

    private void HitAnimace() // animace trefy, hráč při tom mrkne
    {
        CharacterModel.GetComponent<Animations>().MrkniIhned();
        animator.SetTrigger("Hit");
    }
    public void SpustAttackAnimaci(int index, GameObject target) // animace útočení
    {
        animator.SetBool("ReturnToIdle", false);
        animator.SetBool("Walk", false);
        animator.SetInteger("AttackIndex", index);
        animator.SetBool("Attack",true);
        if (index == 0 || index == 4 || index == 5) // konkrétní efekty pro konkrétní postavy a útoky
        {
            Efekty[0].GetComponent<ParticleSystem>().Play();
            lookAtTarget = target.transform;
            lookAtParticle = Efekty[0].GetComponent<ParticleSystem>();
            lookAtTimer = 1f;
        }
        else if (index == 1 || index == 6)
        {
            Efekty[1].GetComponent<ParticleSystem>().Play();
            lookAtTarget = target.transform;
            lookAtParticle = Efekty[1].GetComponent<ParticleSystem>();
            lookAtTimer = 1.3f;
        }
        else if (index == 7)
        {
            Efekty[2].GetComponent<ParticleSystem>().Play();
            lookAtTarget = target.transform;
            lookAtParticle = Efekty[1].GetComponent<ParticleSystem>();
            lookAtTimer = 1.3f;
        }

    }
    void Effects() // výpočet animace efektů
    {
        if (EffectTimer > 0f)
        {
            EffectTimer -= Time.deltaTime;
            DamageEffect.enabled = true;
            DamageEffect.transform.position = EffectStart + new Vector3(0, (EffectTimer) * -1 + 0.5f,1);
            DamageEffect.transform.localScale = Vector3.one * EffectTimer * 2;
        }
        else
        {
            EffectTimer = 0f;
            DamageEffect.enabled = false;
        }
        if (lookAtTimer > 0f && lookAtParticle)
        {
            lookAtTimer -= Time.deltaTime;
            lookAtParticle.transform.LookAt(lookAtTarget);
        }
    }
    // ANIMACE
    public void WalkAnimace()
    {
        animator.SetBool("Walk", true);
        animator.SetBool("Attack", false);
    }
    public void IdleCeka()
    {
        animator.SetBool("Walk", false);
    }

    public void ResetAnimations()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Attack", false);
        animator.SetBool("ReturnToIdle", true);
    }

    public void Defeat()
    {
        animator.SetBool("Defeat", true);
    }
    public void SetGUI() // nastavuje grafiku nad hráčem
    {
        if (HPBar)
        {
            HPBar.maxValue = MaxHP;
            HPBar.value = SmoothHp;
            StaminaBar.maxValue = MaxStamina;
            StaminaBar.value = SmoothStamina;
            StaminaTextFull.text = MaxStamina.ToString();
            StaminaTextCurrent.text = SmoothStamina.ToString();
            HealthTextFull.text = MaxHP.ToString();
            HealthTextCurrent.text = SmoothHp.ToString();
        }
    }
    void PlayEfekt()
    {
        EffectTimer = 1f;
    }
    public void RegenerateStamina() // regenerace energie při bránění
    {
        Stamina += Mathf.Clamp(Mathf.FloorToInt(MaxStamina / 5),0,MaxStamina);
    }
    void Efekt(Color barva, string text, float cas) // spouští efekt
    {
        Invoke("PlayEfekt", cas);
        DamageEffect.color = barva;
        DamageEffect.text = text;
    }
}