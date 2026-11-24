using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image TransitionImage;
    public Image DarkerScreen;
    public TMP_Text ChapterText;

    public RectTransform screen;

    List<string> PopisyKapitol = new List<string>() { 
    "Popis1",
    };

    int currentScreen = 0;
    int currentScreenY = 0;
    float SmoothCurrentScreen = 0;
    float SmoothCurrentScreenY = 0;

    float transTimer = 0f;
    bool TransIsIn = false;
    int vybranaKapitola = 0;

    void Start()
    {
        Application.targetFrameRate = 60;
        DarkerScreen.gameObject.SetActive(true);
        TransitionImage.gameObject.SetActive(true);
        Transition(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (transTimer > 0f)
        {
            TransitionImage.enabled = true;
            transTimer -= Time.deltaTime;
            if (TransIsIn)
            {
                TransitionImage.color = new Color(0,0,0,transTimer);
            }
            else
            {
                TransitionImage.color = new Color(0, 0, 0, 2 - transTimer);
            }
        }
        else
        {
            TransitionImage.enabled = false;
        }
        SmoothCurrentScreen = Mathf.Lerp(SmoothCurrentScreen,currentScreen, Time.deltaTime *3);
        SmoothCurrentScreenY = Mathf.Lerp(SmoothCurrentScreenY,currentScreenY, Time.deltaTime *3);
        screen.anchoredPosition = new Vector2(SmoothCurrentScreen * -800, SmoothCurrentScreenY * 450);
        if (currentScreenY == 1) currentScreen = 1;
        DarkerScreen.color = new Color(0, 0, 0, Mathf.Min(0.6f, SmoothCurrentScreen));
    }
    public void Transition(bool isIn)
    {
        TransIsIn = isIn;
        transTimer = 2f;
    }
    public void PlayButton()
    {
        currentScreen = 1;
    }
    public void Credits()
    {
        currentScreenY = 1;
    }
    public void EndGame()
    {
        Application.Quit();
    }
    public void GamePage()
    {
        Application.OpenURL("strankahry");
    }
    public void Back()
    {
        currentScreen = currentScreen - 1;
        currentScreen = Mathf.Clamp(currentScreen, 0, 2);
    }
    public void Back2()
    {
        currentScreenY = 0;
    }
    public void PlayGame()
    {
        Transition(false);
    }
    void SelectChapter(int chapt)
    {
        currentScreen = 2;
        vybranaKapitola = chapt;
        ChapterText.text = PopisyKapitol[chapt - 1];
    }
    public void Chapt1()
    {
        SelectChapter(1);
    }
    public void Chapt2()
    {
        SelectChapter(2);
    }
    public void Chapt3()
    {
        SelectChapter(3);
    }
    public void Chapt4()
    {
        SelectChapter(4);
    }
    public void Chapt5()
    {
        SelectChapter(5);
    }
}
