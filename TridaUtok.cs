public enum StatusEfekt { Zadny, BolestHlavy, Vystraseny, Povzbuzeni,Mokry,Spanek,Slepota}

[System.Serializable]
public class Utok // Tuto tøídu vygenerovala umìlá inteligence Gemini
{
    public string nazev;
    public string info;
    public int damage;
    public int cost;
    public StatusEfekt status;
    public float sanceNaStatus; // 0.0 až 1.0 (1.0 = 100%)
    public float presnost; // 0.0 až 1.0 (1.0 = 100%)
    public bool vsichni;
    public bool kladny;
    public bool maWalkingAnimaci;
    public int QTE;
    // Konstruktor
    public Utok(string _nazev, int _dmg, int _cost, StatusEfekt _st = StatusEfekt.Zadny, float _sance = 0, float _presnost = 1, bool _vsichni = false, bool _kladny = false, bool _maWalkingAnimaci = false, int qTE = 0, string _info = "")
    {
        nazev = _nazev;
        damage = _dmg;
        cost = _cost;
        status = _st;
        sanceNaStatus = _sance;
        presnost = _presnost;
        vsichni = _vsichni;
        kladny = _kladny;
        maWalkingAnimaci = _maWalkingAnimaci;
        QTE = qTE;
        info = _info;
    }
}