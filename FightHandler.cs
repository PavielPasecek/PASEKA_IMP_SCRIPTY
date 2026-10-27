using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class FightHandler : MonoBehaviour
{
    public enum AttackState { Idle, WalkingToTarget, PerformingAction, ReturningHome,QTE}
    private AttackState currentState = AttackState.Idle;
    public GameObject MakiModel;
    public bool IsRenoInFight;
    public GameObject RenoModel;
    public bool IsAmmaInFight;
    public GameObject AmmaModel;
    public bool IsGrunoInFight;
    public GameObject GrunoModel;

    public List<GameObject> Enemies;
    public int EnemyLevel;
    public bool isBossfight;
    public bool FightEnded;
    public bool IsRunning;

    public List<GameObject> FightMembers;
    public List<GameObject> Heroes;

    public int Turn = 0;
    public GameObject GUI;
    FightGUIHandler guiHandler;

    public AttackHandler FightAttackHandler;
    public FightCameraHandler cameraHandler;
    public QTE QTEHandler;

    public Camera cam;

    private bool turnInitialized = false;

                                         
                                          
    private GameObject activeAttacker;
    private GameObject activeTarget;
    private Vector3 originalPosition;
    private int pendingAttackIndex;
    private int pendingLvl;
    private int pendingStaminaCost;
    private bool isPlayerAttacking;
    private bool pendingAttackType;
    private bool pendingMoreEnemies;
    private float pendingDamageMultiplier;
    private StatusEfekt pendingStatusEffect;

    private GameObject playerCharacter;

    private bool QTEsent = false;

    public List<GameObject> OverworldEnemies;

    public int AfterFight;
    public int AfterFightIndex;

    public GameObject darkness;

    private float darknessTime;
    private bool isDefeated = false;

    public SoundService soundService;
    public CutsceneService cutsceneService;
    void Start()
    {
        CreateFightMembers();
        SkryjSelectedSipky();
        guiHandler = GUI.GetComponent<FightGUIHandler>();
    }
    void EnableAll()
    {
        foreach (GameObject hero in Heroes)
        {
            hero.SetActive(true);
        }
        foreach (GameObject enemy in Enemies)
        {
            enemy.SetActive(true);
        }
        if (darkness)
        {
            darkness.SetActive(false);
        }
    }
    void HandleGameOverAnimation()
    {
        if (darkness)
        {
            float time = Mathf.Clamp(((Time.time - darknessTime) * 0.3f), 0, 1);
            darkness.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, time);
            if (time == 1)
            {
                isDefeated = false;
            }
        }
    }
    void CreateFightMembers()
    {
        // Pøídá všechny èleny teamu a protivníky do listu FightMembers
        FightMembers = new List<GameObject>();
        Heroes = new List<GameObject>();
        if (MakiModel != null) Heroes.Add(MakiModel);
        if (IsRenoInFight && RenoModel != null) Heroes.Add(RenoModel);
        if (IsAmmaInFight && AmmaModel != null) Heroes.Add(AmmaModel);
        if (IsGrunoInFight && GrunoModel != null) Heroes.Add(GrunoModel);
        foreach (GameObject hero in Heroes)
        {
            FightMembers.Add(hero);
            hero.GetComponent<PlayerFight>().StartPlayer(FightAttackHandler);
            hero.SetActive(false);
        }
        foreach (GameObject enemy in Enemies)
        {
            FightMembers.Add(enemy);
            enemy.GetComponent<EnemyFight>().StartEnemy(FightAttackHandler);
            enemy.SetActive(false);
        }
    }

    public void FightZacal()
    {
        // Zapne souboj
        if (playerCharacter)
        {
            playerCharacter.GetComponent<PlayerController>().MainCamera.gameObject.SetActive(false);
        }
        IsRunning = true;
        guiHandler.CurrentFight = gameObject;
        guiHandler.Enemies = Enemies;
        guiHandler.Teammates = Heroes;
        guiHandler.isInFight = true;
        cameraHandler.Setup(cam);
        turnInitialized = false;
        guiHandler.HideStartAnimation();
        Cursor.lockState = CursorLockMode.None;
        EnableAll();
        foreach (GameObject hero in Heroes)
        {
            int heroindex = 1;
            if (hero.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Reno)
                heroindex = 2;
            if (hero.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Amma)
                heroindex = 3;
            if (hero.GetComponent<PlayerFight>().Postava == PlayerFight.Postavy.Gruno)
                heroindex = 4;
            List<GameObject> UI = guiHandler.ReturnPlayerUI(heroindex - 1); // 1 maki, 2 reno, 3 amma, 4 gruno
            hero.GetComponent<PlayerFight>().SetPlayerUI(UI[0],UI[1], UI[2], UI[3], UI[4], UI[5]);
        }
    }
    void HandleButtonPresses() // pro ovládání souboje pøes klávesnici a herní ovladaè
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            guiHandler.ButtonPress('w');
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            guiHandler.ButtonPress('a');
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            guiHandler.ButtonPress('s');
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            guiHandler.ButtonPress('d');
        }
        else if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            guiHandler.ButtonPress('q');
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            guiHandler.ButtonPress('e');
        }
    }

    void Update()
    {
        if (isDefeated)
        {
            HandleGameOverAnimation();
        }
        if (!IsRunning || FightEnded) return;

        if (currentState != AttackState.Idle) // pokud postava nestojí na místì, provede se pohyb
        {
            HandleAttackMovement();
            return;
        }
        GameObject currentCharacter = FightMembers[Turn % FightMembers.Count]; // zjistí, kdo je na tahu
        HandleButtonPresses(); // kontroluje stisknutí kláves
        if (!turnInitialized) // zjistí, jestli byl tah inicializován (tato èást kódu se provede vždy jednou na zaèátku tahu)
        {
            turnInitialized = true;
            SkryjSelectedSipky();
            activeAttacker = currentCharacter;

            if (Heroes.Contains(currentCharacter)) // pokud útoèí hráè
            {
                cameraHandler.SetView(CameraView.Idle,currentCharacter, currentCharacter); // nastaví kameru
                print("ZacatekTahuHrace");
                currentCharacter.GetComponent<PlayerFight>().Guard = false; // vyresetuje obranu
                if (currentCharacter.GetComponent<PlayerFight>().CurrentHP <= 0) // pokud je poražený, pøeskoèí se na dalšího
                {
                    NextTurn();
                    return;
                }
                guiHandler.StartTurnSequence(currentCharacter);// vyresetuje GUI
            }
            else // Pokud je na øadì protivník, udìlá to témìø to samé
            {
                cameraHandler.SetView(CameraView.Idle, activeAttacker, activeAttacker);
                if (currentCharacter.GetComponent<EnemyFight>().CurrentHP <= 0)
                {
                    NextTurn();
                    return;
                }
                currentCharacter.GetComponent<EnemyFight>().Guard = false;
                currentCharacter.GetComponent<EnemyFight>().Enemies = Heroes;
                currentCharacter.GetComponent<EnemyFight>().ChooseAttack(); // vyšle metodu, aby si protivník vybral útok
            }
        }
    }

    void HandleAttackMovement() // tuto metodu na pohyb postavy pøed útokem vytvoøila umìlá inteligence Gemini, QTE jsem pøidal sám bez pomoci
    {
        float speed = 5f;
        Vector3 targetPos;
        if (currentState == AttackState.QTE) // zjistí, jestli má útok QTE
        {
            if (QTEsent == false)
            {
                QTEsent = true;
                QTEHandler.StartQTE(FightAttackHandler.GetQuickTimeEvent(pendingAttackIndex) == 2);
                activeAttacker.GetComponent<PlayerFight>().IdleCeka();
            }
            if (QTEHandler.QTEDone) // zjistí, jestli je QTE hotový
            {
                if (QTEHandler.QTEFailed) // pokud se nepovedl, udìlá útok slabším
                {
                    pendingDamageMultiplier = 0.7f;
                }
                currentState = AttackState.PerformingAction; // ubere životy protivníkovi
                ApplyAttackLogic();
            }
        }
        if (currentState == AttackState.WalkingToTarget)
        {
            float zOffset = isPlayerAttacking ? -1.5f : 1.5f;
            if (isPlayerAttacking) // nastaví správný režim kamery
            {
                cameraHandler.SetView(CameraView.PlayerDash, activeAttacker, activeTarget);
            }
            else
            {
                cameraHandler.SetView(CameraView.EnemyAction, activeAttacker, activeTarget);
            }
            targetPos = activeTarget.transform.position + new Vector3(0, 0, zOffset);
            activeAttacker.transform.position = Vector3.MoveTowards(activeAttacker.transform.position, targetPos, speed * Time.deltaTime); // pohne postavou smìrem k vybranému protivníkovi
            if (Vector3.Distance(activeAttacker.transform.position, targetPos) < 0.05f) // pokud postava došla na vybrané místo
            {
                if (FightAttackHandler.GetQuickTimeEvent(pendingAttackIndex) != 0 && Heroes.Contains(activeAttacker)) // zjistí, jestli má útok QTE a jestli je to hrdina
                {
                    currentState = AttackState.QTE;
                }
                else // pokud ne, provede útok
                {
                    currentState = AttackState.PerformingAction;
                    
                    if (isPlayerAttacking) // nastaví kameru na hráèe nebo protivkíka, na kterého se právì útoèí
                    {
                        if (!pendingAttackType)
                        {
                            cameraHandler.SetView(CameraView.ImpactOnEnemy, activeAttacker, activeTarget);
                        }
                    }
                    else
                    {
                        cameraHandler.SetView(CameraView.EnemyAction, activeAttacker, activeTarget);
                    }
                    ApplyAttackLogic(); // ubere životy
                }
                
            }
        }
        else if (currentState == AttackState.ReturningHome) // pokud se vrací zpìt
        {
            activeAttacker.transform.position = Vector3.MoveTowards(activeAttacker.transform.position, originalPosition, speed * Time.deltaTime);

            if (Vector3.Distance(activeAttacker.transform.position, originalPosition) < 0.05f) // pokud je zpátky na svém místì
            {
                activeAttacker.transform.position = originalPosition;
                currentState = AttackState.Idle; // vyresetuje mu animace, pozici a status chození 
                if (Heroes.Contains(activeAttacker))
                {
                    activeAttacker.GetComponent<PlayerFight>().ResetAnimations();
                }
                else
                {
                    activeAttacker.GetComponent<EnemyFight>().ResetAnimations();
                }
                NextTurn(); // pøedá tah dalšímu
            }
        }
        if (Heroes.Contains(activeAttacker)) // pøehraje animace
        {
            if (currentState == AttackState.WalkingToTarget || currentState == AttackState.ReturningHome)
            {
                activeAttacker.GetComponent<PlayerFight>().WalkAnimace();
            }
        }
        else
        {
            if (currentState == AttackState.WalkingToTarget || currentState == AttackState.ReturningHome)
            {
                activeAttacker.GetComponent<EnemyFight>().WalkAnimace();
            }
        }
    }
    void AttackEnemies(bool all) // ubere životy všem protivníkùm
    {
        if (all)
        {
            foreach (GameObject enemy in Enemies) 
            {
                if (enemy.GetComponent<EnemyFight>().CurrentHP > 0)
                {
                    enemy.GetComponent<EnemyFight>().TakeDamage(pendingAttackIndex, pendingLvl, pendingDamageMultiplier, pendingStatusEffect);
                }
            }
        }
        else
        {
            if (activeTarget.GetComponent<EnemyFight>().CurrentHP > 0) // ubere životy pouze jednomu protivníkovi
            {
                activeTarget.GetComponent<EnemyFight>().TakeDamage(pendingAttackIndex, pendingLvl, pendingDamageMultiplier, pendingStatusEffect);
            }
        }
    }
    void AttackHeroes(bool all)
    {
        if (all)
        {
            foreach (GameObject teammate in Heroes) // vyléèí všechny hrdiny
            {
                if (teammate.GetComponent<PlayerFight>().CurrentHP > 0)
                {
                    teammate.GetComponent<PlayerFight>().TakeDamage(pendingAttackIndex, pendingLvl, pendingStatusEffect);
                }
            }
        }
        else
        {
            if (activeTarget.GetComponent<PlayerFight>().CurrentHP > 0)
            {
                activeTarget.GetComponent<PlayerFight>().TakeDamage(pendingAttackIndex, pendingLvl, pendingStatusEffect);
            }
        }
    }
    void TakeDamage(bool isPlayer,bool jeKladny, bool moreTargets)
    {
        if (isPlayer)
        {
            if (jeKladny)
            {
                AttackHeroes(moreTargets);
            }
            else
            {
                AttackEnemies(moreTargets);
            }
        }
        else
        {
            if (jeKladny)
            {
                AttackEnemies(moreTargets);
            }
            else
            {
                AttackHeroes(moreTargets);
            }
        }
    }
    void ApplyAttackLogic() // vypoèítá útoku a ubere životy
    {
        if (FightAttackHandler.GetDamage(pendingAttackIndex) > 0) // zatøese kamerou
        {
            cameraHandler.TriggerShake(0.5f);
        }
        if (isPlayerAttacking) // pokud útoèí hráè
        {
            activeAttacker.GetComponent<PlayerFight>().SpustAttackAnimaci(pendingAttackIndex,activeTarget); // spustí animace a ubere energii
            activeAttacker.GetComponent<PlayerFight>().Stamina -= pendingStaminaCost;
            activeAttacker.GetComponent<PlayerFight>().SetGUI();
            TakeDamage(true, pendingAttackType, pendingMoreEnemies);
        }
        else // pokud útoèí protivník, logika je úplnì stejná
        {
            activeAttacker.GetComponent<EnemyFight>().SpustAttackAnimaci(pendingAttackIndex);
            activeAttacker.GetComponent<EnemyFight>().Stamina -= pendingStaminaCost;
            TakeDamage(false, pendingAttackType, pendingMoreEnemies);
        }
        if (activeTarget) // zapíše log do konzole
        {
            Debug.Log(activeAttacker.name + " udeøil " + activeTarget.name);
        }
        Invoke("StartReturning", 2f);// zavolá metodu na vrácení se zpìt, pokud postava chodila
    }

    void StartReturning()
    {
        currentState = AttackState.ReturningHome;
    }

    void CheckFightEnd() // zjistí, jestli souboj skonèíl
    {
        int pocetPorazenych = 0;
        foreach (GameObject enemy in Enemies)
        {
            if (enemy.GetComponent<EnemyFight>().CurrentHP <= 0)
            {
                pocetPorazenych++;
            }
        }
        if (pocetPorazenych == Enemies.Count) // zjistí, jestli jsou poražení všichni protivníci
        {
            EndFight(true);
        }
        pocetPorazenych = 0;
        foreach (GameObject hero in Heroes)
        {
            if (hero.GetComponent<PlayerFight>().CurrentHP <= 0)
            {
                pocetPorazenych++;
                hero.GetComponent<PlayerFight>().CharacterModel.GetComponent<Animations>().canBlink = false;
            }
        }
        if (pocetPorazenych == Heroes.Count)  // zjistí, jestli jsou poražení všichni hrdinové
        {
            EndFight(false);
        }
    }

    public void PlayerChoseAttack(int idUtoku, bool naKoho, int indexCile, GameObject postava) // provede útok
    {
        if (idUtoku == -1) // Obrana, žádnú utok se neprovede
        {
            postava.GetComponent<PlayerFight>().RegenerateStamina();
            postava.GetComponent<PlayerFight>().Guard = true;
            NextTurn();
            return;
        }
        // uloží informace o útoku
        activeAttacker = postava;
        isPlayerAttacking = true;
        originalPosition = postava.transform.position;
        pendingAttackIndex = idUtoku;
        pendingLvl = postava.GetComponent<PlayerFight>().Level;
        pendingStaminaCost = FightAttackHandler.Stamina(idUtoku);
        pendingAttackType = FightAttackHandler.GetIsHealing(idUtoku);
        pendingMoreEnemies = FightAttackHandler.GetIsForMoreEnemies(idUtoku);
        pendingDamageMultiplier = 1f;
        pendingStatusEffect = postava.GetComponent<PlayerFight>().aktualniStatus;
        QTEsent = false;
        SkryjSelectedSipky();

        if (indexCile != 0)// zjistí, na koho má otoèit kameru
        {
            activeTarget = naKoho ? Enemies[indexCile - 1] : Heroes[indexCile - 1];
        }
        else // pokud má otoèit kameru na všechny, vybere si jednoho, na kterého ji otoèí
        {
            activeTarget = naKoho ? Enemies[Random.Range(0,Enemies.Count)] : Heroes[Random.Range(0, Heroes.Count)];
        }
        if (FightAttackHandler.GetWalkingAnimation(idUtoku)) // pøehraje chodící animaci, pokud ji má
        {
            currentState = AttackState.WalkingToTarget;
        }
        else
        {
            if (!pendingAttackType) // nastaví kameru
            {
                cameraHandler.SetView(CameraView.ImpactOnFarEnemy, activeAttacker, activeTarget);
            }
            else
            {
                cameraHandler.SetView(CameraView.Idle, activeAttacker, activeAttacker);
            }
            if (FightAttackHandler.GetQuickTimeEvent(idUtoku) != 0) // zjistí, jestli má útok QTE
            {
                currentState = AttackState.QTE;
            }
            else
            {
                ApplyAttackLogic(); // pokud nechodí, ani nemá QTE, aplikuje útok
            }      
        }
    }

    public void EnemyChoseAttack(int idUtoku, GameObject cil, GameObject attacker) // výbìr útoku protivníka
    {
        activeAttacker = attacker;
        isPlayerAttacking = false;
        originalPosition = attacker.transform.position;
        activeTarget = cil;

        if (idUtoku == -1) // Obrana, vynechá útok a regeneruje staminu
        {
            attacker.GetComponent<EnemyFight>().RegenerateStamina();
            attacker.GetComponent<EnemyFight>().Guard = true;
            NextTurn();
            return;
        }
        // uloží informace o útoku do promìnných
        pendingAttackIndex = idUtoku;
        pendingLvl = attacker.GetComponent<EnemyFight>().Level;
        pendingStaminaCost = FightAttackHandler.Stamina(idUtoku);
        pendingAttackIndex = idUtoku;
        pendingAttackType = FightAttackHandler.GetIsHealing(idUtoku);
        pendingMoreEnemies = FightAttackHandler.GetIsForMoreEnemies(idUtoku);
        pendingDamageMultiplier = 1f;
        pendingStatusEffect = attacker.GetComponent<EnemyFight>().aktualniStatus;

        if (FightAttackHandler.GetWalkingAnimation(idUtoku)) // pokud má útok chodící animaci, pøehraje ji
        {
            currentState = AttackState.WalkingToTarget;
        }
        else
        {
            ApplyAttackLogic(); // jinak aplikuje útoku
        }
    }

    private void NextTurn() // pøedá tah další postavì
    {
        CheckFightEnd();
        Turn++;
        turnInitialized = false;
    }

    public void SkryjSelectedSipky() // skryje šipky, které urèují vybraného protivníka
    {
        foreach (GameObject m in Heroes) m.GetComponent<PlayerFight>().SelectIndicator.SetActive(false);
        foreach (GameObject e in Enemies) e.GetComponent<EnemyFight>().SelectIndicator.SetActive(false);
    }

    public void UkazSelectedSipky(int id, bool jeHrac) // ukáže šipky nad vybranou postavou
    {
        if (jeHrac)
        {
            if (Heroes[id].GetComponent<PlayerFight>().CurrentHP > 0)
            {
                cameraHandler.SetView(CameraView.Selection, Heroes[id], Heroes[id]);
                Heroes[id].GetComponent<PlayerFight>().SelectIndicator.SetActive(true);
            }
        }
        else
        {
            if (Enemies[id].GetComponent<EnemyFight>().CurrentHP > 0)
            {
                cameraHandler.SetView(CameraView.Selection, Enemies[id], Enemies[id]);
                Enemies[id].GetComponent<EnemyFight>().SelectIndicator.SetActive(true);
            }
        }
    }

    public void Back()
    {
        cameraHandler.SetView(CameraView.Idle,activeAttacker,activeAttacker);
    }
    public void StartFight(GameObject character = null)
    {
        if (character)
        {
            playerCharacter = character;
            playerCharacter.GetComponent<PlayerController>().GoInFight();
        }
        soundService.PlayMusic();
        guiHandler.PlayStartAnimation();
        Invoke("FightZacal", 1.6f);
    }
    private void OnTriggerEnter(Collider other) // zjistí, jestli se hráè dotkl místa, které zaène souboj
    {
        if (other.CompareTag("Player") && !IsRunning && !FightEnded)
        {
            StartFight(other.gameObject);
        }
    }
    public void PlayDark()
    {
        cutsceneService.PlayDark();
    }

    void AfterWinScene()
    {
        foreach (GameObject enemy in OverworldEnemies)
        {
            enemy.GetComponent<Animator>().SetLayerWeight(enemy.GetComponent<Animator>().GetLayerIndex("Fight"), 1);
            enemy.GetComponent<Animator>().SetBool("Defeat", true);
        }

        if (AfterFight == 0) // Vrátí hráèe do scény
        {
            cameraHandler.StopFightCamera();
            Cursor.lockState = CursorLockMode.Locked;
            playerCharacter.GetComponent<PlayerController>().MainCamera.gameObject.SetActive(true);
            playerCharacter.GetComponent<PlayerController>().isInFight = false;

        }
        else if (AfterFight == 1) // Spustí cutscénu
        {
            cameraHandler.StopFightCamera();
            cutsceneService.PlayCutscene(AfterFightIndex);
        }
    }


    void EndFight(bool vyhralHrac) // ukonèí souboj
    {
        if (vyhralHrac)
        {
            FightEnded = true;
            cameraHandler.SetView(CameraView.EnemyAction, MakiModel, MakiModel);
            if (AfterFight == 0)
            {
                Invoke("AfterWinScene", 5f);
            }
            else if (AfterFight == 1) // Spustí cutscénu
            {
                Invoke("PlayDark", 3f);
                Invoke("AfterWinScene", 5.1f);
            }
        }
        else
        {
            FightEnded = true;
            cameraHandler.SetView(CameraView.Defeat, MakiModel, MakiModel);
            isDefeated = true;
            if (darkness)
            {
                darknessTime = Time.time + 2;
                darkness.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);
                darkness.SetActive(true);
                darkness.transform.position = MakiModel.transform.position;
            }
            guiHandler.Defeat();
        }
        
    }
}