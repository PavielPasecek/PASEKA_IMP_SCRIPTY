using UnityEngine;
using System.Collections.Generic;
public class AttackHandler : MonoBehaviour
{
    // Seznam všech útoků ve hře
    public List<Utok> databazeUtoku = new List<Utok>();

    void Awake()
    {
        /* Jméno útoku, Poškození (záporné pro léčení, následně násobeno levelem), Využití energie, Status, šance na status (vyšší číslo, vyšší šance), 
         * šance na trefu útoku, Je plošný (útočí na všechny nepřátele), Je kladný (Provede se na členy týmu), Má animaci chození, 
         * typ QTE (0 bez QTE, 1 rychlé klikání, 2 - 3 rychlá tlačítka), info k útoku*/
        // MAKI (ID 0-3)
        databazeUtoku.Add(new Utok("Úder pálkou", 30, 0, StatusEfekt.Zadny, 0f, 1f, false, false, true, 1, "Praští pálkou protivníka. Nevyužívá žádnou energii"));
        databazeUtoku.Add(new Utok("Hod sodovkou", 33, 5, StatusEfekt.Zadny, 0.3f, 0.9f, false, false, false, 2, "Hodí na protivníka plechovku se sodou."));
        databazeUtoku.Add(new Utok("Rána do země", 0, 10, StatusEfekt.BolestHlavy, 0.7f, 0f, true, false, false, 0, "Udeří pálkou do země. Její vibrace můžou protivníkům způsobit bolest hlavy"));
        databazeUtoku.Add(new Utok("Svačina", -35, 15, StatusEfekt.Zadny, 0f, 1f, false, true, false, 0, "Vyléčí jednoho člena týmu"));
        // RENO (ID 4-7)
        databazeUtoku.Add(new Utok("Rána kufříkem", 32, 0, StatusEfekt.Zadny, 0f, 1f, false, false, true, 2, "Protivník dostane silnou ránu kufříkem. Bez využití energie"));
        databazeUtoku.Add(new Utok("Kufřík-Combo", 26, 15, StatusEfekt.Zadny, 0f, 0.9f, true, false, true, 1, "Rychlý útok na všechny protivníky, bohužel nepřesný"));
        databazeUtoku.Add(new Utok("Prach z kufříku", 0, 10, StatusEfekt.Slepota, 0.8f, 0f, false, false, true, 0, "Otevře kufřík a vypustí na nepřítele všechen prach. Prach možná udělá protivníka slepým"));
        databazeUtoku.Add(new Utok("Ukolébavka", 0, 12, StatusEfekt.Spanek, 0.33f, 0f, false, false, false, 0, "Vyzvánění telefonu může protivníka uspat"));

        // AMMA a GRUNO - Nepoužité postavy
        databazeUtoku.Add(new Utok("Cupcake", 10, 0));
        databazeUtoku.Add(new Utok("Sušenka", 14, 5));
        databazeUtoku.Add(new Utok("Dort pro všechny", -25, 25, StatusEfekt.Zadny));
        databazeUtoku.Add(new Utok("Dort pro všechny", -25, 25, StatusEfekt.Zadny));
        databazeUtoku.Add(new Utok("Drtič", 45, 30));
        databazeUtoku.Add(new Utok("Zemětřesení", 35, 40));
        databazeUtoku.Add(new Utok("Pohlavek", 8, 5));
        databazeUtoku.Add(new Utok("Pohlavek", 8, 5));

        // Protivníci
        databazeUtoku.Add(new Utok("Normální útok", 28, 0, StatusEfekt.Zadny, 0, 1, false, false, true)); //16
        databazeUtoku.Add(new Utok("Slabý útok", 26, 0, StatusEfekt.Zadny, 0, 1, false, false, true)); //17
        databazeUtoku.Add(new Utok("Silný útok", 31, 0, StatusEfekt.Zadny, 0, 1, false, false, true)); //18
        databazeUtoku.Add(new Utok("Silnější útok", 33, 0, StatusEfekt.Zadny, 0, 1, false, false, true)); //19
        databazeUtoku.Add(new Utok("Nejsilnější útok", 35, 0, StatusEfekt.Zadny, 0, 1, false, false, false)); //20
        databazeUtoku.Add(new Utok("Útok na oba", 27, 0, StatusEfekt.Zadny, 0, 1, true, false, false)); //21
    }

    // Zde jsou metody, které vrací informace o daném útoku
    public string GetNazev(int idUtoku)
    {
        return databazeUtoku[idUtoku].nazev;
    }
    public int GetQuickTimeEvent(int idUtoku)
    {
        return databazeUtoku[idUtoku].QTE;
    }

    public bool GetIsForMoreEnemies(int idUtoku)
    {
        return databazeUtoku[idUtoku].vsichni;
    }

    public bool GetWalkingAnimation(int idUtoku)
    {
        return databazeUtoku[idUtoku].maWalkingAnimaci;
    }

    public int GetDamage(int utok)
    {
        return Mathf.RoundToInt(databazeUtoku[utok].damage);
    }

    public bool GetIsHealing(int idUtoku)
    {
        return databazeUtoku[idUtoku].kladny;
    }

    public int Stamina(int idUtoku)
    {
        return databazeUtoku[idUtoku].cost;
    }
    public StatusEfekt GetStatus(int idUtoku)
    {
        return databazeUtoku[idUtoku].status;
    }
    public float GetPresnost(int idUtoku)
    {
        return databazeUtoku[idUtoku].presnost;
    }
    public float GetStatusChance(int idUtoku)
    {
        return databazeUtoku[idUtoku].sanceNaStatus;
    }
    public string GetAttackInfo(int idUtoku)
    {
        return databazeUtoku[idUtoku].info;
    }
}
