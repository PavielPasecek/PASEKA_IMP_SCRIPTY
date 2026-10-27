using UnityEngine;

public enum CameraView { Idle, Selection, PlayerDash, ImpactOnEnemy, EnemyAction,ImpactOnFarEnemy, Defeat}

public class FightCameraHandler : MonoBehaviour
{
    public CameraView currentView = CameraView.Idle;
    public GameObject attacker;
    public GameObject target;
    public Camera mainCam;

    [Header("Settings")]
    public float followSpeed = 5f;
    public float rotationSpeed = 10f;

    private float shakeTimer;
    private Vector3 shakeOffset;
    private Vector3 currentVelocity; //SmoothDamp

    public void Setup(Camera _cam)
    {
        mainCam = _cam;
        mainCam.gameObject.SetActive(true);
        currentView = CameraView.Idle;
    }

    public void SetView(CameraView view, GameObject _attacker, GameObject _target) // přenastaví momentální typ a pozici
    {
        currentView = view;
        attacker = _attacker;
        target = _target;
    }

    public void TriggerShake(float duration = 0.3f)
    {
        shakeTimer = duration;
    }
    public void StopFightCamera() // vypne kameru (když skončí souboj)
    {
        mainCam.gameObject.SetActive(false);
    }
    private void Update() // s tipy kamer a s plynulým nastavením pozice (metoda SmoothDamp) pomáhala umělá inteligence Gemini. Výpočty pozic jsem vymýšlel sám.
    {
        HandleShake();
        if (target == null) return;


        Vector3 targetPos = transform.position;
        Vector3 lookAtPos = target.transform.position;

        if (currentView == CameraView.Idle) // pohled na hráče, který vybírá útok, nebo na protivníka
        {
            float orbitX = Mathf.Sin(Time.time * 0.2f) * 1.5f;
            float orbitZ = Mathf.Abs(Mathf.Sin(Time.time * 0.2f));
            targetPos = attacker.transform.position + new Vector3(orbitX, 1.2f, -4f + orbitZ);
            lookAtPos = attacker.transform.position;
        }
        else if (currentView == CameraView.Selection) // pohled na cíl, který vybírá hráč
        {
            float orbitX = Mathf.Sin(Time.time * 0.2f) * 1.5f;
            float orbitZ = Mathf.Abs(Mathf.Sin(Time.time * 0.2f));
            targetPos = attacker.transform.position + new Vector3(orbitX, 1.2f, -4f + orbitZ);
            lookAtPos = attacker.transform.position;
        }
        else if (currentView == CameraView.PlayerDash) // pohyb hráče k cíli
        {
            targetPos = attacker.transform.position + new Vector3(2.5f, 1.2f, -2f);
            lookAtPos = target.transform.position;
        }
        else if (currentView == CameraView.ImpactOnFarEnemy) // pohled na cíl, kamera je za hráčem, který se nepohyboval
        {
            Vector3 attackDir = (target.transform.position - attacker.transform.position).normalized;
            targetPos = attacker.transform.position - attackDir * 1.5f + Vector3.up * 1.6f;
            lookAtPos = target.transform.position + Vector3.up * 1.2f;
        }
        else if (currentView == CameraView.ImpactOnEnemy) // pohled na cíl, pokud se hráč pohyboval
        {
            Vector3 attackDir = (target.transform.position - attacker.transform.position).normalized;
            targetPos = attacker.transform.position - attackDir * 1.5f + Vector3.up * 1f;
            lookAtPos = target.transform.position;
        }
        else if (currentView == CameraView.EnemyAction) // pohyb protivníka, který zrovna útočí
        {
            targetPos = attacker.transform.position + new Vector3(-1f, 1.2f, 2.5f);
            lookAtPos = target.transform.position;
        }
        else if (currentView == CameraView.Defeat) // pohyb protivníka, který zrovna útočí
        {
            float orbitX = Mathf.Sin(Time.time * 0.02f) * 1.5f;
            float orbitZ = Mathf.Abs(Mathf.Sin(Time.time * 0.02f));
            targetPos = attacker.transform.position + new Vector3(orbitX, 0.5f, -2f + orbitZ);
            lookAtPos = attacker.transform.position;
        }
        // plynule nastaví pozici a rotaci
        mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, targetPos, ref currentVelocity, 1f / followSpeed);
        Quaternion targetRot = Quaternion.LookRotation(lookAtPos - (mainCam.transform.position + shakeOffset));
        mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    private void HandleShake() // zatřese kamerou při úderu
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float shake = Mathf.Cos(Time.time * 50) * 0.1f;
            float shake2 = Mathf.Sin(Time.time * 50) * 0.1f;
            shakeOffset = new Vector3(shake,shake2,shake);
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }
}