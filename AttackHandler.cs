using UnityEngine;
using System.Collections.Generic;
public class AttackHandler : MonoBehaviour
{
    // Seznam všech útokù ve høe
    public List<Utok> databazeUtoku = new List<Utok>();

    void Awake()
    {
        /* Jméno útoku, Poškození (záporné pro léèení, následnì násobeno levelem), Využití energie, Typ útoku, Status, šance na status (vyšší èíslo, vyšší šance), 
         * šance na trefu útoku, Je plošný (útoèí na všechny nepøátele), Je kladný (Provede se na èleny týmu), Má animaci chození, 
         * typ QTE (0 bez QTE, 1 rychlé klikání, 2 - 3 rychlá tlaèítka), info k útoku*/
        // MAKI (ID 0-3)
        databazeUtoku.Add(new Utok("Úder pálkou", 15, 0, StatusEfekt.Zadny,0f,1f,false,false,true,1,"Praští pálkou protivníka. Nevyužívá žádnou energii"));
        databazeUtoku.Add(new Utok("Hod sodovkou", 16, 5, StatusEfekt.Zadny,0.3f,0.9f,false,false,false,2,"Hodí na protivníka plechovku se sodou."));
        databazeUtoku.Add(new Utok("Rána do zemì", 0, 10,  StatusEfekt.BolestHlavy, 0.7f,0f,true,false,false,0,"Udeøí pálkou do zemì. Její vibrace mùžou protivníkùm zpùsobit bolest hlavy"));
        databazeUtoku.Add(new Utok("Svaèina", -30, 15,  StatusEfekt.Zadny,0f,1f,false,true,false,0,"Vyléèí jednoho èlena týmu"));
        // RENO (ID 4-7)
        databazeUtoku.Add(new Utok("Rána kuføíkem", 18, 0, StatusEfekt.Zadny, 0f, 1f, false, false, true,2,"Protivník dostane silnou ránu kuføíkem. Bez využití energie"));
        databazeUtoku.Add(new Utok("Kuføík-Combo", 10, 15, StatusEfekt.Zadny,0f,0.9f,true,false, true, 1,"Rychlý útok na všechny protivníky, bohužel nepøesný"));
        databazeUtoku.Add(new Utok("Prach z kuføíku", 0, 10,  StatusEfekt.Slepota, 0.8f,0f,false,false,true,0,"Otevøe kuføík a vypustí na nepøítele všechen prach. Prach možná udìlá protivníka slepým"));
        databazeUtoku.Add(new Utok("Ukolébavka", 0, 12, StatusEfekt.Spanek, 0.33f,0f,false,false,false,0,"Vyzvánìní telefonu mùže protivníka uspat"));

        // AMMA a GRUNO - Nepoužité postavy
        databazeUtoku.Add(new Utok("Cupcake", 10, 0));
        databazeUtoku.Add(new Utok("Sušenka", 14, 5));
        databazeUtoku.Add(new Utok("Dort pro všechny", -25, 25, StatusEfekt.Zadny));
        databazeUtoku.Add(new Utok("Dort pro všechny", -25, 25, StatusEfekt.Zadny));
        databazeUtoku.Add(new Utok("Drtiè", 45, 30));
        databazeUtoku.Add(new Utok("Zemìtøesení", 35, 40));
        databazeUtoku.Add(new Utok("Pohlavek", 8, 5));
        databazeUtoku.Add(new Utok("Pohlavek", 8, 5));

        // Protivník
        databazeUtoku.Add(new Utok("Kriket", 10, 0,StatusEfekt.Zadny,0,1,false,false,true)); //16
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