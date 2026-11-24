using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyFight : MonoBehaviour
{
    // tento kód je téměř identický se scriptem PlayerFight.cs
    // jediný rozdíl je ve vybírání útoku a protivníka, to je totiž počítáno hrou (narozdíl od hráče, kde si útok vybírá hráč sám)
    public GameObject CharacterModel;
    public GameObject MomentalniFight;

    public string Name;
    public int MaxStamina;
    public int Stamina;
    public int Level;
    public int MaxHP;
    private int SmoothHp;
    public int CurrentHP;


    public List<int> Utoky;

    public bool Guard = false;
    public StatusEfekt aktualniStatus = StatusEfekt.Zadny;

    public GameObject canvas;
    public Slider HPBar;
    public GameObject SelectIndicator;

    public TMP_Text DamageEffect;
    float EffectTimer = 0f;
    Vector3 EffectStart;

    public List<GameObject> Enemies;

    Vector3 selIndPos;
    int idUtoku;
    GameObject target;
    AttackHandler FightAttackHandler;

    public List<ParticleSystem> StatusParticles;
    private Animator animator;

    public Sprite Icon;

    int statusCounter = 0;

    public enum TypEnemaku {
        Red,
        Blue,
        Violet,
        Yellow,
        Green
    }

    public TypEnemaku TypEnemaka;
    private void Awake()
    {
        animator = CharacterModel.GetComponent<Animator>();
        animator.SetLayerWeight(animator.GetLayerIndex("Fight"), 1);
    }
    public void StartEnemy(AttackHandler a)
    {
        switch(TypEnemaka)
        {
            case TypEnemaku.Red:
            {
                    MaxHP = Mathf.RoundToInt(10 * Mathf.Pow((1.074f), (Level - 1)));
                    break;
            }
            case TypEnemaku.Blue:
            {
                    MaxHP = Mathf.RoundToInt(15 * Mathf.Pow((1.06f), (Level - 1)));
                    break;
            }
            case TypEnemaku.Yellow:
            {
                    MaxHP = Mathf.RoundToInt(8 * Mathf.Pow((1.05f), (Level - 1)));
                    break;
            }
            case TypEnemaku.Violet:
            {
                    MaxHP = Mathf.RoundToInt(10 * Mathf.Pow((1.082f), (Level - 1)));
                    break;
            }
            case TypEnemaku.Green:
            {
                    MaxHP = Mathf.RoundToInt(15 * Mathf.Pow((1.09f), (Level - 1)));
                    break;
            }
        }
        MaxStamina = Mathf.RoundToInt(20 + (1.03f * (Level - 1)));
        Stamina = MaxStamina;
        CurrentHP = MaxHP;
        FightAttackHandler = a;
        SmoothHp = CurrentHP;
        DamageEffect.enabled = false;
        EffectStart = DamageEffect.transform.position;
        selIndPos = SelectIndicator.transform.position;
        SetGUI();
    }
    GameObject ChooseRandomHero()
    {
        if (Enemies.Count == 0) return null;
        int randomIndex = 0;
        do
        {
            randomIndex = Random.Range(0, Enemies.Count);        }
        while (Enemies[randomIndex].GetComponent<PlayerFight>().CurrentHP <= 0);
        return Enemies[randomIndex];
    }

    int ChooseRandomAttack()
    {
        idUtoku = -1;
        int pocetOpakovani = 0;
        do
        {
            idUtoku = Utoky[Random.Range(0, Utoky.Count)];
            print(idUtoku);
            pocetOpakovani++;
        }
        while ((FightAttackHandler.Stamina(idUtoku) > Stamina) && pocetOpakovani < 10);
        if (aktualniStatus == StatusEfekt.Spanek)
        {
            return -1;
        }
        if (aktualniStatus == StatusEfekt.BolestHlavy)
        {
            if (Random.value < 0.5f)
            {
                return -1;
            }
        }
        if (pocetOpakovani >= 10) // guard
        {
            return -1;
        }
        else
        {
            return idUtoku;
        }
    }
    public void ChooseAttack() // výběr náhodného protivníka a útoku
    {
        target = ChooseRandomHero();
        statusCounter--;
        if (statusCounter <= 0) aktualniStatus = StatusEfekt.Zadny;
        if (target != null)
        {
            idUtoku = ChooseRandomAttack();
        }
        Invoke("UkonciTahNepritele", 1.5f);
    }

    private void UkonciTahNepritele() // pošle vybraný tah do Handleru Soubojů
    {
        MomentalniFight.GetComponent<FightHandler>().EnemyChoseAttack(idUtoku, target, gameObject);
    }

    public void TakeDamage(int attackindex, int lvl,float multiplier, StatusEfekt StatusEffect)
    {
        float sanceNaNetrefu = StatusEffect == StatusEfekt.Slepota ? 1f : 0;
        if (Random.value + sanceNaNetrefu > FightAttackHandler.GetPresnost(attackindex) && FightAttackHandler.GetPresnost(attackindex) != 0)
        {
            if (FightAttackHandler.GetDamage(attackindex) > 0)
            {
                Efekt(Color.white, "MIMO", 0.5f);
            }
            return;
        }
        if (Random.value + sanceNaNetrefu > FightAttackHandler.GetStatusChance(attackindex))
        {
            print("status se nepovedl");
            if (FightAttackHandler.GetDamage(attackindex) == 0)
            {
                Efekt(Color.white, "MIMO", 0.5f);
                return;
            }
        }
        else
        {
            print("status se povedl");
            aktualniStatus = FightAttackHandler.GetStatus(attackindex);
            statusCounter = Random.Range(4, 7);
        }
        float gu = Guard ? 0.9f : 1;
        int damage = FightAttackHandler.GetDamage(attackindex);
        int vypocet = Mathf.RoundToInt(Mathf.Pow((Mathf.Abs((float)damage) / 10f), (1 + (float)lvl / 20f)) * gu * multiplier); // výpočet poškození
        if (damage != 0)
        {
            CurrentHP -= vypocet * (damage / Mathf.Abs(damage));
        }
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        SetGUI();
        if (CurrentHP <= 0)
        {
            Defeat();
            Efekt(Color.white, "KO",0.5f);
        }
        else
        {
            if (damage > 0)
            {
                Efekt(Color.red, vypocet.ToString().TrimStart('-'), 0);
                if (!FightAttackHandler.GetIsForMoreEnemies(attackindex))
                {
                    Invoke("HitAnimace", 0.5f);
                }
                else
                {
                    HitAnimace();
                }
            }
            else if (damage != 0 && damage <= 0)
            {
                Efekt(Color.green, vypocet.ToString().TrimStart('-'),0);
            }
        }
    }
    void Particles()
    {
        int targetIndex = (int)aktualniStatus;

        for (int i = 0; i < StatusParticles.Count; i++)
        {
            if (StatusParticles[i] != null)
            {
                if (i == targetIndex)
                {
                    StatusParticles[i].gameObject.SetActive(true);
                    if (!StatusParticles[i].isPlaying)
                    {
                        StatusParticles[i].Play();
                    }
                }
                else
                {
                    StatusParticles[i].gameObject.SetActive(false);
                }
            }
        }
    }
    void Effects()
    {
        Particles();
        if (EffectTimer > 0f)
        {
            EffectTimer -= Time.deltaTime;
            DamageEffect.enabled = true;
            DamageEffect.transform.position = EffectStart + new Vector3(0, (EffectTimer) * -1 + 0.5f, 1);
            DamageEffect.transform.localScale = Vector3.one * EffectTimer * 2;
        }
        else
        {
            EffectTimer = 0f;
            DamageEffect.enabled = false;
        }
    }
    private void LateUpdate()
    {
        if (Camera.main)
        {
            canvas.transform.LookAt(canvas.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up); // otočí grafiku směrem ke kameře
        }
        CharacterModel.transform.position = transform.position + Vector3.down;
        CharacterModel.transform.rotation = new Quaternion(0, 0, 0, 0);
        if (SmoothHp != CurrentHP)
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
        Effects();
        SelectIndicator.transform.position = selIndPos + Vector3.up * Mathf.Sin(Time.time * 2) * 0.03f;
        animator.SetLayerWeight(animator.GetLayerIndex("Fight"), 1);
    }

    private void HitAnimace()
    {
        animator.SetTrigger("Hit");
    }
    public void SpustAttackAnimaci(int index)
    {
        animator.SetInteger("AttackIndex", index);
        animator.SetTrigger("Attack");
    }
    public void ResetAnimations()
    {
        animator.ResetTrigger("Attack");
    }

    public void Defeat()
    {
        animator.SetTrigger("Defeat");
    }
    void SetGUI()
    {
        HPBar.maxValue = MaxHP;
        HPBar.value = SmoothHp;
    }
    public void RegenerateStamina()
    {
        //Stamina += Mathf.Clamp(Mathf.FloorToInt(MaxStamina / 4), 0, MaxStamina);
        Stamina += Mathf.Clamp(5, 0, MaxStamina);
    }
    void PlayEfekt()
    {
        EffectTimer = 1f;
    }
    void Efekt(Color barva, string text, float cas)
    {
        Invoke("PlayEfekt", cas);
        DamageEffect.color = barva;
        DamageEffect.text = text;
    }
}
