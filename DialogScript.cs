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
    public Postavy postava;
    public Sprite Base;
    public Sprite Eyes;
    public Sprite Eyebrows;
    public Sprite Mouth;
    public Sprite TalkingMouth;
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
        if (index == -1)
        {
            DialogBody.GetComponent<Image>().sprite = null;
            DialogShadow.GetComponent<Image>().sprite = null;
            DialogEyebrows.GetComponent<Image>().sprite = null;
            DialogEyes.GetComponent<Image>().sprite = null;
            return;
        }
        if (index > Faces.Count - 1 || index < 0)
        {
            return;
        }
        DialogBody.GetComponent<Image>().sprite = Faces[index].Base;
        DialogShadow.GetComponent<Image>().sprite = Faces[index].Base;
        DialogEyebrows.GetComponent<Image>().sprite = Faces[index].Eyebrows;
        DialogEyes.GetComponent<Image>().sprite = Faces[index].Eyes;
        DialogMouth.GetComponent<Image>().sprite = Faces[index].Mouth;
        StoredIndex = index;
        

    }
    // Update is called once per frame
    void Update()
    {
        AnimateMouth = MomentalniZprava != null && zpravaIndex < MomentalniZprava.Length;
        if (MomentalniZprava != null && zpravaIndex < MomentalniZprava.Length)
        {
            if (letterTime <= 0f)
            {
                CS.PlayTalkSound();
                if (MomentalniZprava[zpravaIndex] == '.')
                {
                    letterTime = 0.2f;
                }
                else if (MomentalniZprava[zpravaIndex] == ',')
                {
                    letterTime = 0.1f;
                }
                else
                {
                    letterTime = 0.05f;
                }
                TextDialogu.text = MomentalniZprava.Substring(0,zpravaIndex + 1);
                zpravaIndex++;
            }
        }
        if (letterTime > 0f)
        {
            letterTime -= Time.deltaTime;
        }
        if (StoredIndex >= 0)
        {
            if (AnimateMouth)
            {
                currentActor.Talk(Time.time * 5 % 2 > 1, StoredIndex);
                DialogMouth.GetComponent<Image>().sprite = Time.time * 5 % 2 > 1 ? Faces[StoredIndex].Mouth : Faces[StoredIndex].TalkingMouth;
            }
            else
            {
                currentActor.Talk(false, StoredIndex);
                DialogMouth.GetComponent<Image>().sprite = Faces[StoredIndex].Mouth;
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && MomentalniZprava != null)
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
