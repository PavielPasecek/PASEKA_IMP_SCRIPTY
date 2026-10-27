using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightGUIHandler : MonoBehaviour
{
    public bool isInFight = false;
    public GameObject CurrentFight; // objekt moment·lnÌho souboje, nastavÌ jin˝ kÛd
    public GameObject ActiveCharacter; // moment·lnÌ aktivnÌ postava, nastavÌ jin˝ kÛd

    [Header("Hracovi Utoky")]
    public GameObject Utoky; // seznam hr·Ëov˝ch ˙tok˘, nastavÌ jin˝ kÛd

    [Header("Seznamy")]
    public List<GameObject> Teammates;
    public List<GameObject> Enemies;
    public GameObject TeammateList;
    public GameObject EnemyList;

    [Header("Tlacitka a volby")]
    public GameObject ProceedButton; // tlaËÌtko na potvrzenÌ ˙toku
    public GameObject BackButton; // tlaËÌtko na vr·cenÌ o krok zpÏt
    public GameObject ZakladniVolba; // 3 tlaËÌtka, ˙tok, obrana a kapsy
    public GameObject Guard; // okno s dialogem, objevÌ se po v˝bÏru "Obrana"

    public GameObject IntroAnim; // animace na zaË·tku souboje

    public GameObject Info; // informace o danÈm ˙toku
    public TMP_Text InfoText; // text v informacÌch
    public TMP_Text InfoPower; // sÌla v informacÌch
    public TMP_Text InfoStamina; // energie v informacÌch
    public TMP_Text InfoStatus; // status v informacÌch

    public GameObject GameOverButtons; // tlaËÌtka vzd·t a zpÏt do menu

    public AttackHandler FightAttackHandler; // Attack Handler, aby si kÛd mohl zjiöùovat informace o ˙tocÌch

    int VybranyUtok = 0;
    int VybranyCil = 0;
    bool utokJeNaEnemy = true;
    bool AllEnemiesUtok = false;

    public enum MenuState { Skryto, ZakladniVolba, VyberUtoku, VyberCile, Pripraveno, PotvrzeniGuard }
    public MenuState currentState = MenuState.Skryto;
    public enum TargetType { Enemy, Teammate, AllEnemies }

    private List<List<GameObject>> HeroIconsUI;
    public List<GameObject> Hero1Icons;
    public List<GameObject> Hero2Icons;
    public List<GameObject> Hero3Icons;
    public List<GameObject> Hero4Icons;

    public List<Sprite> AttackIcons;

    public GameObject CharIcons;
    private Animator CharIconAnims;


    void Start()
    {
        Teammates = new List<GameObject>();
        Enemies = new List<GameObject>();
        UpdateUI();
        HideStartAnimation();
        HeroIconsUI = new List<List<GameObject>>();
        HeroIconsUI.Add(Hero1Icons);
        HeroIconsUI.Add(Hero2Icons);
        HeroIconsUI.Add(Hero3Icons);
        HeroIconsUI.Add(Hero4Icons);
        CharIconAnims = CharIcons.GetComponent<Animator>();
        GameOverButtons.SetActive(false);
    }

    public void StartTurnSequence(GameObject character)
    {
        ActiveCharacter = character;
        currentState = MenuState.ZakladniVolba;
        UpdateUI();
    }
    public List<GameObject> ReturnPlayerUI(int heroIndex) // p¯ed· postavÏ v bitvÏ jeho UI
    {
        return HeroIconsUI[heroIndex];
    }
    private void UpdateUI()
    {
        ZakladniVolba.SetActive(false);
        Utoky.SetActive(false);
        EnemyList.SetActive(false);
        TeammateList.SetActive(false);
        ProceedButton.SetActive(false);
        BackButton.SetActive(false);
        Guard.SetActive(false);
        Info.SetActive(false);
        UpdateHeroList();
        if (currentState == MenuState.ZakladniVolba) // zobrazÌ z·kladnÌ volbu (˙tok, obrana, kapsy)
        {
            ZakladniVolba.SetActive(true);
            CharIconAnims.SetBool("Down",true);
            CharIconAnims.SetBool("Up",false);
        }
        else if (currentState == MenuState.VyberUtoku) // zobrazÌ 4 ˙toky hr·Ëe a tlaËÌtko zpÏt
        {
            UpdateAttackList();
            Utoky.SetActive(true);
            BackButton.SetActive(true);
            Info.SetActive(false);
            CharIconAnims.SetBool("Down", true);
            CharIconAnims.SetBool("Up", false);
        }
        else if (currentState == MenuState.Pripraveno || currentState == MenuState.VyberCile) // nech· hr·Ëe vybrat cÌl a n·slednÏ provÈst ˙tok
        {
            CharIconAnims.SetBool("Down", false);
            CharIconAnims.SetBool("Up", true);
            Info.SetActive(true);
            if (utokJeNaEnemy)
            {
                EnemyList.SetActive(true);
                UpdateEnemyList();
            }
            else
            {
                TeammateList.SetActive(true);
            }
            BackButton.SetActive(true);
            if (currentState == MenuState.Pripraveno)
            {
                ProceedButton.transform.Find("Energie").GetComponent<TMP_Text>().text = "-" + FightAttackHandler.Stamina(VybranyUtok).ToString();
                ProceedButton.transform.Find("Energie").GetComponent<TMP_Text>().color = new Color(0, 0.9450981f,1);
                ProceedButton.SetActive(true);
                if (AllEnemiesUtok)
                {
                    EnemyList.SetActive(false);
                    TeammateList.SetActive(false);
                }
            }
        }
        else if (currentState == MenuState.PotvrzeniGuard)
        {
            Guard.SetActive(true);

        }
    }

    public void OnClickAttack() { currentState = MenuState.VyberUtoku; UpdateUI(); }
    public void OnClickGuard() { currentState = MenuState.PotvrzeniGuard; UpdateUI(); }

    private TargetType CheckAttack(int utokID) // ZjistÌ, jestli je ˙tok kladn˝ a jestli se provede na vÌce protivnÌk˘
    {
        if (FightAttackHandler.GetIsHealing(utokID))
        {
            return TargetType.Teammate;
        }
        else
        {
            if (FightAttackHandler.GetIsForMoreEnemies(utokID))
            {
                return TargetType.AllEnemies;
            }
            else
            {
                return TargetType.Enemy;
            }
        }
    }

    private void SelectorOnAll(bool heroes) // zobrazÌ öipky na vöech postav·ch
    {
        CurrentFight.GetComponent<FightHandler>().SkryjSelectedSipky();
        if (heroes)
        {
            for (int i = 0; i < Teammates.Count; i++)
            {
                CurrentFight.GetComponent<FightHandler>().UkazSelectedSipky(i,true);
            }
        }
        else
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                CurrentFight.GetComponent<FightHandler>().UkazSelectedSipky(i, false);
            }
        }
    }
    void Selector(int index,bool heroes) // zobrazÌ öipky
    {
        CurrentFight.GetComponent<FightHandler>().SkryjSelectedSipky();
        CurrentFight.GetComponent<FightHandler>().UkazSelectedSipky(index, heroes);
    }
    

    public void VybranUtok(int cisloUtoku) // podle stisknutÌ tlaËÌtka vybere ˙tok a zjistÌ mu ID
    {
        int postava = 0;
        if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Reno)
        {
            postava = 1;
        }
        else if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Amma)
        {
            postava = 2;
        }
        else if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Gruno)
        {
            postava = 3;
        }
        VybranyUtok = cisloUtoku + (4 * postava);
        TargetType typ = CheckAttack(VybranyUtok);
        utokJeNaEnemy = (typ == TargetType.Enemy || typ == TargetType.AllEnemies);
        AllEnemiesUtok = (typ == TargetType.AllEnemies);
        SetInfoAboutAttack(VybranyUtok);
        if (AllEnemiesUtok)
        {
            SelectorOnAll(!utokJeNaEnemy);
            VybranyCil = 0;
            currentState = MenuState.Pripraveno;
        }
        else
        {;
            currentState = MenuState.VyberCile;
        }
        UpdateUI();
    }

    public void VybranUtok1() { VybranUtok(0); }
    public void VybranUtok2() { VybranUtok(1); }
    public void VybranUtok3() { VybranUtok(2); }
    public void VybranUtok4() { VybranUtok(3); }
    public void VybranCil(int cisloCile) // vybere cÌl
    {
        if (utokJeNaEnemy) // zjistÌ, jestli nenÌ poraûen˝
        {
            if (Enemies[cisloCile - 1].GetComponent<EnemyFight>().CurrentHP <= 0)
            {
                return;
            }
        }
        else
        {
            if (Teammates[cisloCile - 1].GetComponent<PlayerFight>().CurrentHP <= 0)
            {
                return;
            }
        }
        VybranyCil = cisloCile; // vybere cÌl a p¯ipravÌ ˙tok
        Selector(cisloCile - 1,!utokJeNaEnemy);
        currentState = MenuState.Pripraveno;
        UpdateUI();
    }

    public void VybranEnemy1() { VybranCil(1); }
    public void VybranEnemy2() { VybranCil(2); }
    public void VybranEnemy3() { VybranCil(3); }
    public void VybranEnemy4() { VybranCil(4); }
    public void VybranTeammateMaki() { VybranCil(1); }
    public void VybranTeammateReno() { VybranCil(2); }
    public void VybranTeammateAmma() { VybranCil(3); }
    public void VybranTeammateGruno() { VybranCil(4); }

    public void ButtonPress(char tlacitko)
    {
        if (currentState == MenuState.VyberUtoku)
        {
            if (tlacitko == 'w')
            {
                VybranUtok(0);
            }
            else if (tlacitko == 's')
            {
                VybranUtok(2);
            }
            else if (tlacitko == 'a')
            {
                VybranUtok(1);
            }
            else if (tlacitko == 'd')
            {
                VybranUtok(3);
            }
            else if (tlacitko == 'q')
            {
                Back();
            }
        }
        else if (currentState == MenuState.VyberCile || (currentState == MenuState.Pripraveno && !AllEnemiesUtok)) // pokud vybÌr·m cÌl, nebo potvrzuji ˙tok
        {
            if (utokJeNaEnemy)
            {

                if (tlacitko == 'w' && Enemies.Count >= 4)
                {
                    VybranCil(4);
                }
                else if (tlacitko == 's' && Enemies.Count >= 1)
                {
                    VybranCil(1);
                }
                else if (tlacitko == 'a' && Enemies.Count >= 2)
                {
                    VybranCil(2);
                }
                else if (tlacitko == 'd' && Enemies.Count >= 3)
                {
                    VybranCil(3);
                }
            }
            else
            {
                if (tlacitko == 'w' && Teammates.Count >= 4)
                {
                    VybranCil(4);
                }
                else if (tlacitko == 's' && Teammates.Count >= 1)
                {
                    VybranCil(1);
                }
                else if (tlacitko == 'a' && Teammates.Count >= 2)
                {
                    VybranCil(2);
                }
                else if (tlacitko == 'd' && Enemies.Count >= 3)
                {
                    VybranCil(3);
                }
            }
            if (tlacitko == 'q')
            {
                Back();
            }
        }
        else if (currentState == MenuState.ZakladniVolba) // pokud vybÌr·m ˙tok, kapsy nebo obranu
        {
            if (tlacitko == 'w')
            {
                currentState = MenuState.VyberUtoku; 
                UpdateUI();
            }
            else if (tlacitko == 'd')
            {
                currentState = MenuState.PotvrzeniGuard;
                UpdateUI();
            }
        }


        if (currentState == MenuState.Pripraveno)
        {
            if (tlacitko == 'e')
            {
                FireAttack();
            }
            else if (tlacitko == 'q')
            {
                Back();
            }
        }

    }
    
    public void GuardConfirmYes() // pokud se mÌsto ˙toku chceme br·nit
    {
        CurrentFight.GetComponent<FightHandler>().PlayerChoseAttack(-1, false, 0, ActiveCharacter);
        EndTurn();
    }
    public void GuardConfirmNo() { Back(); } // vr·tÌ zp·tky o jeden krok

    public void Back()
    {
        if (currentState == MenuState.VyberUtoku || currentState == MenuState.PotvrzeniGuard)
            currentState = MenuState.ZakladniVolba;
        else if (currentState == MenuState.VyberCile || currentState == MenuState.Pripraveno)
        {
            currentState = MenuState.VyberUtoku;
            CurrentFight.GetComponent<FightHandler>().Back();
        }
        CurrentFight.GetComponent<FightHandler>().SkryjSelectedSipky();
        UpdateUI();
    }

    public void FireAttack() // vyöle sign·l, ûe hr·Ë vybral ˙tok a m˘ûe se tak provÈst dalöÌ Ë·st boje
    {
        if (FightAttackHandler.Stamina(VybranyUtok) <= ActiveCharacter.GetComponent<PlayerFight>().Stamina) // kontrola, dostatek staminy
        {
            CharIconAnims.SetBool("Down", true);
            CharIconAnims.SetBool("Up", false);
            CurrentFight.GetComponent<FightHandler>().PlayerChoseAttack(VybranyUtok, utokJeNaEnemy, VybranyCil, ActiveCharacter); // poöle metodu
            EndTurn();
        }
    }

    private void EndTurn()
    {
        currentState = MenuState.Skryto;
        UpdateUI();
    }

    public void UpdateEnemyList() // obsah tÈto metody byl generov·n umÏlou inteligencÌ Gemini
    {
        foreach (Transform child in EnemyList.transform) child.gameObject.SetActive(false); // skryje vöechny tlaËÌtka s protivnÌky
        for (int i = 0; i < Enemies.Count; i++)
        {
            Transform btn = EnemyList.transform.Find("Enemy" + (i + 1).ToString());
            if (btn != null && Enemies[i].GetComponent<EnemyFight>().CurrentHP > 0) // uk·ûe pouze protivnÌky, kte¯Ì nejsou poraûenÌ
            {
                btn.gameObject.SetActive(true);
                //btn.Find("Text").GetComponent<TMP_Text>().text = Enemies[i].GetComponent<EnemyFight>().Name; pozdÏji ikonka
            }
        }
    }

    public void UpdateHeroList() // obsah tÈto metody byl generov·n umÏlou inteligencÌ Gemini
    {
        foreach (Transform child in TeammateList.transform) child.gameObject.SetActive(false); // skryje vöechny tlaËÌtka s Ëleny t˝mu
        foreach (Transform child in CharIcons.transform) child.gameObject.SetActive(false); // skryje vöechny ûivoty Ëlen˘ t˝mu (vytvo¯eno mnou)
        foreach (GameObject teammate in Teammates)
        {
            if (teammate.GetComponent<PlayerFight>().CurrentHP > 0) // uk·ûe pouze Ëleny t˝mu, kte¯Ì nejsou poraûenÌ
            {
                string expectedBtn = teammate.name.Replace("Fight", "Button"); // UdÏl· ze stringu JmÈnoFight JmÈnoButton
                Transform btn = TeammateList.transform.Find(expectedBtn);
                if (btn != null) btn.gameObject.SetActive(true);
            }
            CharIcons.transform.Find(teammate.name.Replace("Fight","")).gameObject.SetActive(true); // (vytvo¯eno mnou)
        }
    }
    void SetInfoAboutAttack(int attackId) // nastavuje informace o jednotliv˝ch ˙tocÌch
    {
        //Textov˝ popis
        InfoText.text = FightAttackHandler.GetAttackInfo(attackId).ToString();
        // sÌla
        InfoPower.text = "SÌla:" + FightAttackHandler.GetDamage(attackId).ToString().TrimStart('-');
        
        if (FightAttackHandler.GetDamage(attackId) > 0)
        {
            InfoPower.color = Color.red;
        }
        else if (FightAttackHandler.GetDamage(attackId) < 0)
        {
            InfoPower.color = Color.green;
        }
        else
        {
            InfoPower.text = "";
        }
        // energie
        InfoStamina.text = "Energie:" + FightAttackHandler.Stamina(attackId).ToString();
        if (FightAttackHandler.Stamina(attackId) > 0)
        {
            InfoStamina.color = new Color(0, 1, 0.979579f);
        }
        else
        {
            InfoStamina.text = "";
        }
        // status ˙toku
        if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Povzbuzeni)
        {
            InfoStatus.text = "+ PovzbuzenÌ";
            InfoStatus.color = Color.yellow;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Vystraseny)
        {
            InfoStatus.text = "+ VystraöenÌ";
            InfoStatus.color = Color.mediumPurple;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.BolestHlavy)
        {
            InfoStatus.text = "+ Bolest hlavy";
            InfoStatus.color = Color.red;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Mokry)
        {
            InfoStatus.text = "+ Mokr˝";
            InfoStatus.color = Color.lightSkyBlue;
        }
        else
        {
            InfoStatus.text = "";
        }
    }
    public void UpdateAttackList() // zjistÌ, jak· je moment·lnÌ pouûÌvan· postava a podle nÌ uk·ûe vöechny jejÌ ˙toky
    {
        int postava = 0;
        if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Reno)
        {
            postava = 1;
        }
        else if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Amma)
        {
            postava = 2;
        }
        else if (ActiveCharacter.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Gruno)
        {
            postava = 3;
        }
        foreach (Transform child in Utoky.transform) child.gameObject.SetActive(false);
        for (int i = 0; i < Utoky.transform.childCount; i++)
        {
            Transform child = Utoky.transform.GetChild(i);
            int attackNum = child.gameObject.name[child.gameObject.name.Length - 1] - '0';
            int attackId = attackNum + (postava * 4) - 1;
            child.gameObject.SetActive(true);
            if (AttackIcons[attackId] != null)
            {
                GameObject attackIcon = child.GetChild(0).gameObject;
                attackIcon.GetComponent<Image>().sprite = AttackIcons[attackId];
            }
        }
    }

    public void PlayStartAnimation() // spustÌ intro animaci
    {
        IntroAnim.SetActive(true);
        IntroAnim.GetComponent<Animator>().Play("IntroAnim");
        CharIconAnims.SetBool("Down", true);
        CharIconAnims.SetBool("Up", false);
    }
    void Hide() // skryje intro animaci
    {
        IntroAnim.SetActive(false);
    }
    public void BackToMenu()
    {
        GameOverButtons.SetActive(false);
        CurrentFight.GetComponent<FightHandler>().PlayDark();
        Invoke("StartMenuScene", 2f);
    }
    void StartMenuScene()
    {
        SceneManager.LoadScene(0);
    }
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Again()
    {
        GameOverButtons.SetActive(false);
        CurrentFight.GetComponent<FightHandler>().PlayDark();
        Invoke("ReloadScene", 2f);
    }
    public void Defeat()
    {
        CharIcons.SetActive(false);
        GameOverButtons.SetActive(true);
    }
    public void HideStartAnimation() // po 0.2 vte¯iny skryje intro animaci
    {
        Invoke("Hide",0.2f);
    }
}