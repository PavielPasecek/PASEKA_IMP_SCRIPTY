using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class SceneDialog
{
    public GameObject actor; // momentální postava v dialogu
    public string Text; // text který říká
    public string ActorName; // jméno postavy v dialogu
    public int DialogIconNum; // jakou ikonku má postava ukazovat
    public Vector3 CameraPos; // pozice kamery
    public Vector3 CameraLook; // směr kamery 
    public bool AnimationTrigger; // pokud má přehrát animaci na postavě
    public int SpecialIndex; // pro speciální efekty
    public bool ChangePos; // pozice
    public Vector3 Position;
    public bool ChangeRot; // rotace
    public Vector3 Rotation;
}
[System.Serializable]
public class Scene
{
    public int MusicIndex; // jaká hudba má hrát
    public List<GameObject> actors; // všechny modely postav ve scéně
    public List<SceneDialog> dialogList; // všechny dialogy
    public int NextPartIndex; // co se má stát po cutscéně
    public GameObject NextPart;
}


public class CutsceneService : MonoBehaviour
{
    public List<Scene> scenes; // list všech cutscén ve scéně
    public DialogScript Dialog; // Dialogové okno
    public Image Transition; // přechod (černá barva)
    bool isDialogPlaying = false; // pokud hraje dialog
    int DialogIndex = 0; // kolikátý dialog hraje
    int SceneIndex = 0; // kolikátá scéna

    float transitionTime = 0f;
    bool IsTransition = true;
    bool IsDialogEnding = false;
    bool DoSomethingAfterTransition = false;

    public GameObject FightUI;

    public Camera mainCamera;

    public SoundService soundService;

    public int StartCutscene = -1;
    public int CollisionCutscene = -1;
    public SpecialEffectsService specialEffectService;

    PlayerController playerscript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (StartCutscene != -1)
        {
            PlayCutscene(StartCutscene);
        }
        else
        {
            PlayDarkIn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsTransition)
        {
            if (isDialogPlaying)
            {
                if (transitionTime > 0)
                {
                    transitionTime -= Time.deltaTime;
                    Transition.color = new Color(0, 0, 0, transitionTime);
                    Transition.gameObject.SetActive(true);
                }
                else
                {
                    Transition.gameObject.SetActive(false);
                    if (DoSomethingAfterTransition)
                    {
                        LoadDialog();
                    }
                }
            }
            else if (IsDialogEnding)
            {
                if (transitionTime > 0)
                {
                    transitionTime -= Time.deltaTime;
                    Transition.color = new Color(0, 0, 0,1 - transitionTime);
                    Transition.gameObject.SetActive(true);
                }
                else
                {
                    if (DoSomethingAfterTransition)
                    {
                        EndCutscene();
                    }
                }
            }
        }
    }
    void TurnOffCamera()
    {
        mainCamera.gameObject.SetActive(false);
        Transition.gameObject.SetActive(false);
    }
    void TurnOffCameraAfterTime(float i)
    {
        Invoke("TurnOffCamera", i);
    }
    void EndCutscene()
    {
        if (IsDialogEnding)
        {
            Scene scene = scenes[SceneIndex];
            IsDialogEnding = false;
            print("konec cutsceny");
            if (scene.NextPartIndex == 0) // Nechá po scéně hráče ovládat postavu
            {
                TurnOffCamera();
                if (scene.NextPart != null)
                {
                    PlayDarkIn();
                    playerscript = scene.NextPart.GetComponent<PlayerController>();
                    playerscript.MainCamera.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    playerscript.isInFight = false;
                }
            }
            else if (scene.NextPartIndex == 1) // Spustí po scéně souboj
            {
                TurnOffCameraAfterTime(1.7f);
                scene.NextPart.GetComponent<FightHandler>().StartFight();
                FightUI.SetActive(true);
            }
            else if (scene.NextPartIndex >= 10) // Spustí po cutséně další scénu, index scény je NextPartIndex - 10
            {
                SceneManager.LoadScene(scene.NextPartIndex - 10);
            }
        }
    }
    void EndDialog()
    {
        print("dialog ending");
        DoSomethingAfterTransition = true;
        isDialogPlaying = false;
        IsDialogEnding = true;
        IsTransition = true;
        transitionTime = 1f;
        soundService.StopVoices();
        Dialog.gameObject.SetActive(false);
    }
    void LoadDialog()
    {
        IsTransition = false;
        Dialog.gameObject.SetActive(true);
        Scene scene = scenes[SceneIndex];
        if (scene.dialogList.Count - 1 < DialogIndex)
        {
            EndDialog();
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        SceneDialog currentDialog = scene.dialogList[DialogIndex];
        if (currentDialog == null) return;
        mainCamera.transform.position = currentDialog.CameraPos;
        mainCamera.transform.LookAt(currentDialog.CameraLook);
        if (currentDialog.ChangePos)
        {
            currentDialog.actor.transform.position = currentDialog.Position;
        }
        if (currentDialog.ChangeRot)
        {
            currentDialog.actor.transform.rotation = Quaternion.Euler(currentDialog.Rotation);
        }
        if (currentDialog.SpecialIndex > 0)
        {
            specialEffectService.PlaySpecialScene(currentDialog.SpecialIndex);
        }
        Dialog.Dialog(currentDialog.ActorName, currentDialog.Text, currentDialog.DialogIconNum);
        if (currentDialog.actor)
        {
            Animations actorAnimations = currentDialog.actor.GetComponent<Animations>();
            if (actorAnimations)
            {
                Dialog.Dialog3DAnimation(actorAnimations);
            }     
        }
        else
        {
            Dialog.Dialog3DAnimation(null);
        }
    }
    void PrepareDialog()
    {
        FightUI.SetActive(false);
        Scene scene = scenes[SceneIndex];
        SceneDialog currentDialog = scene.dialogList[DialogIndex];
        mainCamera.gameObject.SetActive(true);
        mainCamera.transform.position = currentDialog.CameraPos;
        mainCamera.transform.LookAt(currentDialog.CameraLook);
        foreach (GameObject actor in scene.actors)
        {
            if (actor != null)
            {
                actor.SetActive(true);
            }
        }
    }
    public void PlayCutscene(int sceneIndex)
    {
        if (sceneIndex >= scenes.Count)
        {
            return;
        }
        transitionTime = 3f;
        isDialogPlaying = true;
        DoSomethingAfterTransition = true;
        DialogIndex = 0;
        SceneIndex = sceneIndex;
        PrepareDialog();
    }

    public void PlayDark()
    {
        isDialogPlaying = false;
        IsDialogEnding = true;
        IsTransition = true;
        transitionTime = 2f;
        DoSomethingAfterTransition = false;
    }
    public void PlayDarkIn()
    {
        isDialogPlaying = true;
        IsDialogEnding = false;
        IsTransition = true;
        transitionTime = 2f;
        DoSomethingAfterTransition = false;
    }

    public void Next()
    {
        if (isDialogPlaying && !IsTransition)
        {
            DialogIndex++;
            LoadDialog();
        }
    }

    public void PlayTalkSound()
    {
        soundService.PlayTalkSound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameObject player = other.gameObject;
            playerscript = player.GetComponent<PlayerController>();
            if (!playerscript.isInFight)
            {
                gameObject.GetComponent<Collider>().enabled = false;
                playerscript.GoInFight();
                PlayDark();
                Invoke("PlayCollisionCutscene", 4f);
            }
        }
    }

    void PlayCollisionCutscene()
    {
        PlayCutscene(CollisionCutscene);
        if (playerscript != null)
        {
            playerscript.MainCamera.gameObject.SetActive(false);
        }
    }
}
