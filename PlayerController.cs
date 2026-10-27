using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float gravity = -9.81f;

    public Camera MainCamera;

    private CharacterController controller;
    private float yVelocity;
    private float moveSpeed = 6f;
    public GameObject CharacterRig;
    public GameObject Weaponp1;
    public GameObject Weaponp2;
    private Animator animator;
    public bool isInFight = false;

    // nastavení pro kameru
    public float mouseSensitivity = 2.0f;
    public float cameraDistance = 6.0f;
    public float cameraHeight = 1f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    // proměnné pro výpočet kamery
    private float yaw = 0f;
    private float pitch = 0f;

    float yawChange;
    float pitchChange;

    // proměnné pro pohyb postay
    float xaxis = 0;
    float yaxis = 0;

    public bool DungeonAnimations;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = CharacterRig.GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isInFight) // chceme pohybovat postavou pouze, pokud není ve stavu boje
        {
            Move();
            HandleCamera();
        }
    }

    void Move()
    {
        // s otáčením kamery a následným pohybem postavy směrem, kterým míří kamera, pomáhala umělá inteligence Gemini

        Vector3 camForward = MainCamera.transform.forward; // výpočet pohybu směrem kamery
        Vector3 camRight = MainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 move = camRight * xaxis + camForward * yaxis;
        float inputMagnitude = Mathf.Clamp01(move.magnitude); // výpočet rychlosti postavy, 
        yVelocity += gravity * Time.deltaTime; // výpočet gravitace
        // nastavení pozice a rotace modelu postavy
        CharacterRig.transform.position = transform.position + Vector3.down;
        if (move.magnitude > 0.03f)
        {
            CharacterRig.transform.rotation = Quaternion.Slerp(CharacterRig.transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 15); // otočení postavy směrem chození
        }
        move = move.normalized; // normalizace pohybum aby při diagonálním chození nebyla postava rychlejší
        Vector3 velocity = moveSpeed * (DungeonAnimations ? 1 : 0.6f) * move + Vector3.up * yVelocity; 
        controller.Move(velocity * Time.deltaTime);
        animator.SetFloat("Speed", inputMagnitude); // animace chození
        CharacterRig.GetComponent<Animations>().speed = inputMagnitude; // přeposlání rychlosti (pro animace)
        animator.SetLayerWeight(animator.GetLayerIndex("City"), DungeonAnimations ? 0 : 1);
        Weaponp1.gameObject.SetActive(DungeonAnimations);
        Weaponp2.gameObject.SetActive(DungeonAnimations);
    }

    void HandleCamera()
    {
        yaw += yawChange;
        pitch -= pitchChange;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // clamp, proti pohledu moc nahoru nebo dolu
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0); 
        Vector3 targetPivot = transform.position + Vector3.up * cameraHeight;
        Vector3 Position = targetPivot - (rotation * Vector3.forward * cameraDistance); // výpočet kamery
        MainCamera.transform.position = Position;
        MainCamera.transform.rotation = rotation; // nastavení rotace a pozice
    }
    public void GoInFight() // nastaví postavu do stavu boje
    {
        isInFight = true;
        animator.SetFloat("Speed", 0);
    }

    private void OnMove(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>(); // nastaví proměnné, pokud byly stisknuty klávesy pro pohyb
        xaxis = movement.x;
        yaxis = movement.y;
    }

    private void OnLook(InputValue value) // nastaví proměnné, pokud bylo pohnuto myší, nebo pravým joystickem
    {
        Vector2 look = value.Get<Vector2>();
        yawChange = look.x;
        pitchChange = look.y;
    }
}
