using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightGUIHandler : MonoBehaviour
{
    public bool isInFight = false;
    public GameObject CurrentFight; // objekt momentálního souboje, nastaví jiný kód
    public GameObject ActiveCharacter; // momentální aktivní postava, nastaví jiný kód

    [Header("Hracovi Utoky")]
    public GameObject Utoky; // seznam hráčových útoků, nastaví jiný kód

    [Header("Seznamy")]
    public List<GameObject> Teammates;
    public List<GameObject> Enemies;
    public GameObject TeammateList;
    public GameObject EnemyList;

    [Header("Tlacitka a volby")]
    public GameObject ProceedButton; // tlačítko na potvrzení útoku
    public GameObject BackButton; // tlačítko na vrácení o krok zpět
    public GameObject ZakladniVolba; // 3 tlačítka, útok, obrana a kapsy
    public GameObject Guard; // okno s dialogem, objeví se po výběru "Obrana"

    public GameObject IntroAnim; // animace na začátku souboje

    public GameObject Info; // informace o daném útoku
    public TMP_Text InfoText; // text v informacích
    public TMP_Text InfoPower; // síla v informacích
    public TMP_Text InfoStamina; // energie v informacích
    public TMP_Text InfoStatus; // status v informacích

    public GameObject GameOverButtons; // tlačítka vzdát a zpět do menu

    public AttackHandler FightAttackHandler; // Attack Handler, aby si kód mohl zjišťovat informace o útocích

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
        Application.targetFrameRate = 60;
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
    public List<GameObject> ReturnPlayerUI(int heroIndex) // předá postavě v bitvě jeho UI
    {
        return HeroIconsUI[heroIndex];
    }
    void HideCharIcons()
    {
        CharIconAnims.SetBool("Down", true);
        CharIconAnims.SetBool("Up", false);
    }
    void ShowCharIcons()
    {
        CharIconAnims.SetBool("Down", false);
        CharIconAnims.SetBool("Up", true);
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
        if (currentState == MenuState.ZakladniVolba) // zobrazí základní volbu (útok, obrana)
        {
            ZakladniVolba.SetActive(true);
            HideCharIcons();
        }
        else if (currentState == MenuState.VyberUtoku) // zobrazí 4 útoky hráče a tlačítko zpět
        {
            UpdateAttackList();
            Utoky.SetActive(true);
            BackButton.SetActive(true);
            Info.SetActive(false);
            HideCharIcons();
        }
        else if (currentState == MenuState.Pripraveno || currentState == MenuState.VyberCile) // nechá hráče vybrat cíl a následně provést útok
        {
            ShowCharIcons();
            Info.SetActive(true);
            if (utokJeNaEnemy) // pokud je útok na protivníky
            {
                EnemyList.SetActive(true);
                UpdateEnemyList();
            }
            else // pokud je útok na spoluhráče (léčení)
            {
                TeammateList.SetActive(true);
            }
            BackButton.SetActive(true);
            if (currentState == MenuState.Pripraveno) // zobrazí tlačítko na zahájení útoku
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
        else if (currentState == MenuState.PotvrzeniGuard) // obrana
        {
            Guard.SetActive(true);

        }
    }

    public void OnClickAttack() { currentState = MenuState.VyberUtoku; UpdateUI(); }
    public void OnClickGuard() { currentState = MenuState.PotvrzeniGuard; UpdateUI(); }

    private TargetType CheckAttack(int utokID) // Zjistí, jestli je útok kladný a jestli se provede na více protivníků
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

    private void SelectorOnAll(bool heroes) // zobrazí šipky na všech postavách
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
    void Selector(int index,bool heroes) // zobrazí šipky
    {
        CurrentFight.GetComponent<FightHandler>().SkryjSelectedSipky();
        CurrentFight.GetComponent<FightHandler>().UkazSelectedSipky(index, heroes);
    }
    

    public void VybranUtok(int cisloUtoku) // podle stisknutí tlačítka vybere útok a zjistí mu ID
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
    public void VybranCil(int cisloCile) // vybere cíl
    {
        if (utokJeNaEnemy) // zjistí, jestli není poražený
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
        VybranyCil = cisloCile; // vybere cíl a připraví útok
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
        else if (currentState == MenuState.VyberCile || (currentState == MenuState.Pripraveno && !AllEnemiesUtok)) // pokud vybírám cíl, nebo potvrzuji útok
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
        else if (currentState == MenuState.ZakladniVolba) // pokud vybírám útok, kapsy nebo obranu
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
    
    public void GuardConfirmYes() // pokud se místo útoku chceme bránit
    {
        CurrentFight.GetComponent<FightHandler>().PlayerChoseAttack(-1, false, 0, ActiveCharacter);
        EndTurn();
    }
    public void GuardConfirmNo() { Back(); } // vrátí zpátky o jeden krok

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

    public void FireAttack() // vyšle signál, že hráč vybral útok a může se tak provést další část boje
    {
        if (FightAttackHandler.Stamina(VybranyUtok) <= ActiveCharacter.GetComponent<PlayerFight>().Stamina) // kontrola, dostatek staminy
        {
            CharIconAnims.SetBool("Down", true);
            CharIconAnims.SetBool("Up", false);
            CurrentFight.GetComponent<FightHandler>().PlayerChoseAttack(VybranyUtok, utokJeNaEnemy, VybranyCil, ActiveCharacter); // pošle metodu
            EndTurn();
        }
    }

    private void EndTurn()
    {
        currentState = MenuState.Skryto;
        UpdateUI();
    }

    public void UpdateEnemyList() // obsah této metody byl generován umělou inteligencí Gemini
    {
        foreach (Transform child in EnemyList.transform) child.gameObject.SetActive(false); // skryje všechny tlačítka s protivníky
        for (int i = 0; i < Enemies.Count; i++)
        {
            Transform btn = EnemyList.transform.Find("Enemy" + (i + 1).ToString());
            if (btn != null && Enemies[i].GetComponent<EnemyFight>().CurrentHP > 0) // ukáže pouze protivníky, kteří nejsou poražení
            {
                btn.gameObject.SetActive(true);
                btn.Find("Image").GetComponent<Image>().sprite = Enemies[i].GetComponent<EnemyFight>().Icon;
            }
        }
    }

    public void UpdateHeroList() // obsah této metody byl generován umělou inteligencí Gemini
    {
        foreach (Transform child in TeammateList.transform) child.gameObject.SetActive(false); // skryje všechny tlačítka s členy týmu
        foreach (Transform child in CharIcons.transform) child.gameObject.SetActive(false); // skryje všechny životy členů týmu (vytvořeno mnou)
        foreach (GameObject teammate in Teammates)
        {
            if (teammate.GetComponent<PlayerFight>().CurrentHP > 0) // ukáže pouze členy týmu, kteří nejsou poražení
            {
                string expectedBtn = teammate.name.Replace("Fight", "Button"); // Udělá ze stringu JménoFight JménoButton
                Transform btn = TeammateList.transform.Find(expectedBtn);
                if (btn != null) btn.gameObject.SetActive(true);
            }
            CharIcons.transform.Find(teammate.name.Replace("Fight","")).gameObject.SetActive(true); // (vytvořeno mnou)
        }
    }
    void SetInfoAboutAttack(int attackId) // nastavuje informace o jednotlivých útocích
    {
        //Textový popis
        InfoText.text = FightAttackHandler.GetAttackInfo(attackId).ToString();
        // síla
        InfoPower.text = "Síla:" + FightAttackHandler.GetDamage(attackId).ToString().TrimStart('-');
        
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
        // status útoku
        if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Povzbuzeni)
        {
            InfoStatus.text = "+ Povzbuzení";
            InfoStatus.color = Color.yellow;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Vystraseny)
        {
            InfoStatus.text = "+ Vystrašení";
            InfoStatus.color = Color.mediumPurple;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.BolestHlavy)
        {
            InfoStatus.text = "+ Bolest hlavy";
            InfoStatus.color = Color.red;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Mokry)
        {
            InfoStatus.text = "+ Mokrý";
            InfoStatus.color = Color.lightSkyBlue;
        }
        else if (FightAttackHandler.GetStatus(attackId) == StatusEfekt.Slepota)
        {
            InfoStatus.text = "+ Slepota";
            InfoStatus.color = Color.grey;
        }
        else
        {
            InfoStatus.text = "";
        }
    }
    public void UpdateAttackList() // zjistí, jaká je momentální používaná postava a podle ní ukáže všechny její útoky
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

    public void PlayStartAnimation() // spustí intro animaci
    {
        IntroAnim.SetActive(true);
        IntroAnim.GetComponent<Animator>().Play("IntroAnim");
        HideCharIcons();
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
    public void HideStartAnimation() // po 0.2 vteřiny skryje intro animaci
    {
        Invoke("Hide",0.2f);
    }
}
