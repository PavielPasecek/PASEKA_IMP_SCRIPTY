using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QTE : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool QTEStarted = false;
    public GameObject RychleQTE;
    public GameObject RychleQTECircle;
    public GameObject RychleQTEKey;

    public GameObject TriQTE;
    public GameObject TriQTEButton1;
    public GameObject TriQTEButton2;
    public GameObject TriQTEButton3;
    public Slider TriQTESlider; 

    int RychleMaxQTE = 3;
    int QTEClickCount = 0;
    char Key = '0';
    private List<KeyCode> tlacitka;
    KeyCode spravneTlacitko;

    int TriMaxQTE = 4;
    int fazeTri = 0;
    KeyCode spravneTlacitko1;
    KeyCode spravneTlacitko2;
    KeyCode spravneTlacitko3;

    float momentalniCas = 0f;

    bool QTEType = false; // false Rychle, true 3
    public bool QTEFailed = false;
    public bool QTEDone = false;
    void Start()
    {
        RychleQTE.SetActive(false);
        TriQTE.SetActive(false);
        tlacitka = new List<KeyCode>();
        tlacitka.Add(KeyCode.W);
        tlacitka.Add(KeyCode.S);
        tlacitka.Add(KeyCode.A);
        tlacitka.Add(KeyCode.D);
    }
    void EndQTE()
    {
        QTEStarted = false;
        QTEDone = true;
        RychleQTE.SetActive(false);
        TriQTE.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (QTEStarted)
        {
            momentalniCas += Time.deltaTime;
            if (!QTEType) // pokud je typ 1 náhodné tlačítko
            {
                // animace tlačítka
                float rychlost = 1 + Mathf.Sin(momentalniCas * 20) * 0.1f;
                RychleQTECircle.GetComponent<Image>().fillAmount = 1- momentalniCas / RychleMaxQTE;
                RychleQTECircle.gameObject.transform.localScale = Vector3.one * rychlost;
                RychleQTEKey.gameObject.transform.parent.transform.localScale = Vector3.one * rychlost;
                //zjištení vstupu
                if (Input.GetKeyDown(spravneTlacitko))
                {
                    QTEClickCount++;
                    if (QTEClickCount > RychleMaxQTE * 3) // zjišťujě, jestli byl dosažen dostatek kliknutí
                    {
                        QTEFailed = false;
                        EndQTE(); // ukončí QTE
                    }
                }
                else
                {
                    for (int i = 0; i < tlacitka.Count - 1; i++) // zjistí, jestli nebylo stisknuto chybné tlačítko
                    {
                        if (Input.GetKeyDown(tlacitka[i]) && tlacitka[i] != spravneTlacitko)
                        {
                            EndQTE();
                        }
                    }
                }
                if (momentalniCas >= RychleMaxQTE) // pokud dojde čas, ukončí se
                {
                    EndQTE();
                }
            }
            else
            {
                // zobrazí správný počet tlačítek a nastaví časomíru
                TriQTESlider.maxValue = TriMaxQTE;
                TriQTESlider.value = TriMaxQTE - momentalniCas;
                if (fazeTri == 1)
                {
                    spravneTlacitko = spravneTlacitko1;
                }
                else if (fazeTri == 2)
                {
                    spravneTlacitko = spravneTlacitko2;
                    TriQTEButton1.gameObject.SetActive(false);
                }
                else if (fazeTri == 3)
                {
                    spravneTlacitko = spravneTlacitko3;
                    TriQTEButton2.gameObject.SetActive(false);
                }
                if (Input.GetKeyDown(spravneTlacitko)) // pokud bylo stisknuto správné tlačítko
                {
                    fazeTri++;
                }
                else
                {
                    for (int i = 0; i < tlacitka.Count - 1; i++) // pokud bylo stisknuto špatné
                    {
                        if (Input.GetKeyDown(tlacitka[i]) && tlacitka[i] != spravneTlacitko)
                        {
                            EndQTE();
                        }
                    }
                }
                if (fazeTri >= 4) // pokud hráč správně stiskl 3 tlačítka za sebou
                {
                    QTEFailed = false;
                    EndQTE();
                }
                if (momentalniCas >= TriMaxQTE)
                {
                    EndQTE();
                }
            }
        }
    }
    public void StartQTE(bool typ)
    {
        QTEStarted = true;
        QTEType = typ;
        momentalniCas = 0f;
        if (!QTEType) // pokud je typ QTE 1, dostanete jedno náhodné tlačítko, které musíte rychle mačkat
        {
            RychleQTE.SetActive(true);
            QTEClickCount = 0;
            spravneTlacitko = tlacitka[Random.Range(0, tlacitka.Count)]; // vybere náhodné klávesy (W,S,A,D)
            Key = spravneTlacitko.ToString()[0];
            RychleQTEKey.gameObject.GetComponent<TMP_Text>().text = Key.ToString();
        }
        else // pokud je typ QTE sekvence 3 kláves, které musíte zmáčknout za sebou 
        {
            TriQTE.SetActive(true);
            fazeTri = 1;
            spravneTlacitko1 = tlacitka[Random.Range(0, tlacitka.Count)]; // vybere 3 náhodné klávesy
            spravneTlacitko2 = tlacitka[Random.Range(0, tlacitka.Count)];
            spravneTlacitko3 = tlacitka[Random.Range(0, tlacitka.Count)];
            TriQTEButton1.gameObject.transform.Find("Text").GetComponent<TMP_Text>().text = spravneTlacitko1.ToString();
            TriQTEButton2.gameObject.transform.Find("Text").GetComponent<TMP_Text>().text = spravneTlacitko2.ToString();
            TriQTEButton3.gameObject.transform.Find("Text").GetComponent<TMP_Text>().text = spravneTlacitko3.ToString();
            TriQTEButton1.gameObject.SetActive(true);
            TriQTEButton2.gameObject.SetActive(true);
            TriQTEButton3.gameObject.SetActive(true);
        }
        QTEFailed = true;
        QTEDone = false;
    }
}