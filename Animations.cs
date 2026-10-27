using UnityEngine;
using System.Collections.Generic;

public class Animations : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> Emotions;
    public List<GameObject> Mouths;
    public GameObject Teeth;
    public GameObject AroundEyes;
    public List<GameObject> Blink;
    public List<GameObject> OtherEyelids;
    public List<GameObject> Weapons;
    public List<GameObject> Eyes;
    public bool HasSpecialAnimation;
    public GameObject SpecialAnimationNormal;
    public GameObject SpecialAnimationMoving;
    public Animator SpecialAnimationAnimator;

    public float speed = 0;
    public enum Postavy
    {
        Maki,
        Amma,
        Reno,
        Gruno,
        Otec,

    }
    public Postavy Postava;
    public bool canBlink = true;
    float BlinkCooldown = 1;
    void Start()
    {
        HideAllEmotions();
        HideAllMouths();
        HideAllBlinking();
        SetAnimation(0);
    }


    public void MrkniIhned() // je zavolaná při útoku na postavu, aby postava při úderu mrkla
    {
        BlinkCooldown = 0.3f;
    }
    void HideAllMouths()
    {
        foreach (GameObject obj in Mouths)
        {
            obj.gameObject.SetActive(false);
        }
        
    }

    void HideAllEmotions()
    {
        foreach (GameObject obj in Emotions)
        {
            obj.gameObject.SetActive(false);
        }
        
    }


    void HideAllBlinking()
    {
        foreach (GameObject obj in Blink)
        {
            obj.gameObject.SetActive(false);
        }
        foreach (GameObject obj in OtherEyelids)
        {
            obj.gameObject.SetActive(false);
        }
        AroundEyes.gameObject.SetActive(false);
    }

    void ManageBlinking() // zpracovává mrkání postavy
    {
        if (canBlink)
        {
            BlinkCooldown -= Time.deltaTime;
            if (BlinkCooldown < 0.3f)
            {
                if (BlinkCooldown < 0.3f && BlinkCooldown > 0.2f)
                {
                    HideAllBlinking();
                    Blink[0].gameObject.SetActive(true);
                }
                else if (BlinkCooldown < 0.2f && BlinkCooldown > 0.1f)
                {
                    HideAllBlinking();
                    Blink[1].gameObject.SetActive(true);
                }
                else if (BlinkCooldown < 0.1f && BlinkCooldown > 0)
                {
                    HideAllBlinking();
                    Blink[0].gameObject.SetActive(true);
                }
                else if (BlinkCooldown < 0)
                {
                    BlinkCooldown = Random.Range(10,15);
                    HideAllBlinking();
                    AroundEyes.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            HideAllBlinking();
            Blink[1].gameObject.SetActive(true);
        }
    }

    void ManageSpecialAnimations(Postavy p) // pro speciální animace (mikina)
    {
        if (p == Postavy.Maki)
        {
            SpecialAnimationAnimator.SetBool("Walking", speed > 0.8f);
            SpecialAnimationMoving.SetActive(speed > 0.8f);
            SpecialAnimationNormal.SetActive(!(speed > 0.8f));
        }
    }
    public void SetAnimation(int num) // nastaví výraz na postavě, podle výrazu spritu postavy v dialogu
    {
        if (Postava == Postavy.Maki)
        {
            HideAllMouths();
            HideAllEmotions();
            if (num == 1 || num == 5 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[2].gameObject.SetActive(true);
            }
            else if (num == 2 || num == 4)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[1].gameObject.SetActive(true);
            }
            else if (num == 3 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[2].gameObject.SetActive(true);
            }
            else
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[0].gameObject.SetActive(true);
            }
        }
        else if (Postava == Postavy.Reno)
        {
            HideAllMouths();
            HideAllEmotions();
            if (num == 1 || num == 5 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[2].gameObject.SetActive(true);
            }
            else if (num == 2 || num == 4)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[1].gameObject.SetActive(true);
            }
            else if (num == 3 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[2].gameObject.SetActive(true);
            }
            else
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[0].gameObject.SetActive(true);
            }
        }
        else if (Postava == Postavy.Otec)
        {
            HideAllMouths();
            HideAllEmotions();
            if (num == 1 || num == 5 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[1].gameObject.SetActive(true);
            }
            else if (num == 2 || num == 4)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[1].gameObject.SetActive(true);
            }
            else if (num == 3 || num == 7)
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[2].gameObject.SetActive(true);
            }
            else
            {
                Mouths[0].gameObject.SetActive(true);
                Emotions[0].gameObject.SetActive(true);
            }
        }
    }

    public void Talk(bool open, int num) // otevře postavě pusu, pokud právě mluví
    {
        HideAllMouths();
        if (open)
        {
            Mouths[3].gameObject.SetActive(true);
        }
        else
        {
            SetAnimation(num);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Eyes.Count > 1)
        {
            Eyes[1].gameObject.SetActive(false);
        }
        ManageBlinking();
        ManageSpecialAnimations(Postava);
    }
}
