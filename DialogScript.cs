using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public enum Postavy
{
    Maki,
    Amma,
    Reno,
    Gruno
}
[System.Serializable]
public class DialogFace
{
    public Sprite Base; // základní sprite postavy
    public Sprite Eyes; // oči postavy
    public Sprite Eyebrows; // obočí postavy
    public Sprite Mouth; // pusa postavy
    public Sprite TalkingMouth; // animační snímek otevřené pusy
}
/*
 Maki - Normální 0
    Smutný 1 
    Naštvaný 2  
    Nechápající 3
    Konfidentní 4
    Šťastný 5
    NevimNeco 6 
    Vyděšený 7
 */
public class DialogScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public TMP_Text TextDialogu;
    public TMP_Text JmenoPostavy;
    float letterTime = 0f;
    int zpravaIndex = 0;
    string MomentalniZprava;

    public GameObject DialogBody;
    public GameObject DialogEyes;
    public GameObject DialogEyebrows;
    public GameObject DialogShadow;
    public GameObject DialogMouth;
    public CutsceneService CS;
    Animations currentActor;

    public List<DialogFace> Faces;

    bool AnimateMouth = false;

    int StoredIndex;

    public void Dialog(string jmeno,string zprava, int obrazek)
    {
        zpravaIndex = 0;
        JmenoPostavy.text = jmeno;
        MomentalniZprava = zprava;
        NactiObrazekPostavy(obrazek);
    }
    public void Dialog3DAnimation(Animations actor)
    {
        currentActor = actor;
    }
    void Start()
    {

    }
    private void NactiObrazekPostavy(int index)
    {
        if (index > Faces.Count - 1 || index < 0) // pokud index neexistuje, vykreslí se prázdný sprite 16
        {
            index = 16;
        }
        DialogBody.GetComponent<Image>().sprite = Faces[index].Base;
        DialogShadow.GetComponent<Image>().sprite = Faces[index].Base;
        DialogEyebrows.GetComponent<Image>().sprite = Faces[index].Eyebrows;
        DialogEyes.GetComponent<Image>().sprite = Faces[index].Eyes;
        DialogMouth.GetComponent<Image>().sprite = Faces[index].Mouth;
        StoredIndex = index; // uloží index
        

    }
    // Update is called once per frame
    void Update()
    {
        AnimateMouth = MomentalniZprava != null && zpravaIndex < MomentalniZprava.Length; // určuje, jestli má animovat ústa
        if (MomentalniZprava != null && zpravaIndex < MomentalniZprava.Length) // pokud momentální text existuje a není už vypsaný
        {
            if (letterTime <= 0f) // letterTime označuje čas na vypsání každého písmena dialogu
            {
                CS.PlayTalkSound(); // přehraje zvuk
                if (MomentalniZprava[zpravaIndex] == '.') // tečky mají nejdelší čas výpisu
                {
                    letterTime = 0.2f;
                }
                else if (MomentalniZprava[zpravaIndex] == ',') // čárky o polovinu menší
                {
                    letterTime = 0.1f;
                }
                else // všechna ostatní písmena se vypisují normální rychlostí
                {
                    letterTime = 0.05f;
                }
                TextDialogu.text = MomentalniZprava.Substring(0,zpravaIndex + 1); // vypíše momentální část dialogu
                zpravaIndex++;
            }
        }
        if (letterTime > 0f) // odpočítává čas
        {
            letterTime -= Time.deltaTime;
        }
        if (StoredIndex >= 0) // pokud existuje konkrétní portrét
        {
            bool openMouth = Time.time * 5 % 2 > 1; // přepíná zhruba 3x za sekundu
            if (AnimateMouth) // pokud má animovat ústa
            {
                if (currentActor) // pokud existuje 3D model postavy
                {
                    currentActor.Talk(openMouth, StoredIndex); // otevře / zavře ústa u 3D modelu
                }
                // to samé u portrétu
                DialogMouth.GetComponent<Image>().sprite = openMouth ? Faces[StoredIndex].Mouth : Faces[StoredIndex].TalkingMouth;
            }
            else
            {
                if (currentActor)
                {
                    currentActor.Talk(false, StoredIndex); // vykreslí 3D postavě zavřená ústa
                }
                DialogMouth.GetComponent<Image>().sprite = Faces[StoredIndex].Mouth; // to samé vykreslí portrétu
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && MomentalniZprava != null) // přepne na další zprávu
        {
            Next();
        }
    }

    public void Next()
    {
        if (CS)
        {
            if (!AnimateMouth)
            {
                CS.Next();
            }
            else
            {
                zpravaIndex = MomentalniZprava.Length - 2;
            }
        }
    }

}
