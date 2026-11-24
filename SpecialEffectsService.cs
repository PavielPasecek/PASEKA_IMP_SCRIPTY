using System.Collections.Generic;
using UnityEngine;

public class SpecialEffectsService : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    List<int> Akce;
    public List<int> AkceCasy;
    public float time = 0f;
    int current_index = -1;
    public List<GameObject> effects;
    void Start()
    {
        Akce = new List<int>();
        /*
            1 - otevření portálu
         */
        AkceCasy = new List<int>() {0,2,5}; // akce jsou od nuly, nultá se ale nepočítá
    }

    // Update is called once per frame
    void Update()
    {
        if (Akce.Count > 0)
        {
            VykonatAkci();
            Cas();
        }
    }

    void Cas()
    {
        if (time > 0f)
        {
            time -= Time.deltaTime;
            if (time < 0f)
            {
                Akce.RemoveAt(0);
            }
        }
    }

    void VykonatAkci()
    {
        if (Akce[0] != current_index)
        {
            current_index = Akce[0];
            time = AkceCasy[current_index];
            switch (current_index)
            {
                case 1:
                    effects[0].GetComponent<Animator>().SetTrigger("Play");
                    break;

            }
        }
    }

    public void PlaySpecialScene(int index)
    {
        Akce.Add(index);
    }
}
