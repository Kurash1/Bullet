using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;

public class Buttons : MonoBehaviour
{
    [SerializeField] Button play;
    [SerializeField] Button help;
    [SerializeField] Button close;
    [SerializeField] InputField name_in;
    [SerializeField] GameObject helpmenu;
    [SerializeField] Text highscore;
    [SerializeField] Text worldhighscore;
    public static Button infobtn;
    public static Text infotxt;
    public static string[] abilities = { "Shotgun", "Slow Time", "Necromancy", "Wall", "Turret", "Machine Gun", "Comet", "Fire Circle", "Rocket Launcher", "Ghost", "Alluring Scent", "Teleport", "Slash" };
    public static string[] upgrades = { "Aura of Hatred", "Back-Up Mag", "Companion", "Shotgun", "Dual Wield", "Ghost Hunter", "Graveyard", "House", "Landmine", "Shoulder Gun" };
    public static Dictionary<string, string> ability_descriptions = new Dictionary<string, string>();
    public static Dictionary<string, string> upgrade_descriptions = new Dictionary<string, string>();
    // Start is called before the first frame update
    IEnumerator GetWorlScore()
    {
        string uri = "http://jesser.vlab.fi/PHP/bullet.php";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    string body = new Regex("<body>\\n.*</body>").Match(webRequest.downloadHandler.text).Value;
                    //body = new Regex(".^(<body>)").Match(body).Value;
                    body = Regex.Replace(body, "<br>", "\n");
                    body = Regex.Replace(body, "<body>", "");
                    body = Regex.Replace(body, "</body>", "");
                    worldhighscore.text = body;
                    break;
            }
        }
    }
    void WriteAbility(string key, string value) { if(!Buttons.ability_descriptions.ContainsKey(key)) { Buttons.ability_descriptions.Add(key, value); } }
    void WriteUpgrade(string key, string value) { if(!Buttons.upgrade_descriptions.ContainsKey(key)) { Buttons.upgrade_descriptions.Add(key, value); } }
    void Start()
    {
        WriteAbility("Shotgun","The Shotgun allows you to fire off many bullets in a cone towards your mouse position.");
        WriteAbility("Slow Time","Slows down time for a short period of time.");
        WriteAbility("Necromancy", "Necromancy raises previously killed enemies as bullets that fly towards your mouse position.");
        WriteAbility("Wall", "The Wall creates two walls to the front and back of your character.");
        WriteAbility("Turret", "You summon a turret that shoots in random directions, this turrent doesn't provide points, but does provide abilities.");
        WriteAbility("Machine Gun", "Loads/Reloads 30 bullets to your machine gun which will be consantly shot towards your mouse position.");
        WriteAbility("Comet", "Makes your character into a comet firing off short lived bullets in all directions for a short duration.");
        WriteAbility("Fire Circle", "Creates a firey circle that spreads out from your character.");
        WriteAbility("Rocket Launcher", "Shoots out a bullet that explodes on contact.");
        WriteAbility("Ghost", "Turns your character into a ghost for a short duration, in ghost form you are immortal, and your bullets can additionally kill ghost type enemies.");
        WriteAbility("Alluring Scent", "Spawns an Alluring Scent that pulls simple movement enemies towards it.");
        WriteAbility("Teleport", "Teleports you to your mouse position and kill everything between your old and new positions.");
        WriteAbility("Slash", "Grants you 5 slashes you can use to slash with your right click.");
        WriteUpgrade("Aura of Hatred", "Your Alluring Scent now repels enemies");
        WriteUpgrade("Back-Up Mag",  "When you use a machine gun you are given an additonal 30 regular ammo, Additionally you can right-click to give all of your ammo to your machine gun");
        WriteUpgrade("Dual Wield",  "Machine gun shoots two bullets every time now");
        WriteUpgrade("Ghost Hunter",  "Allows your regular bullets to kill ghosts");
        WriteUpgrade("Graveyard",  "Doubles the amount of undead summoned by necromancy");
        WriteUpgrade("House",  "Changes wall to build a house around you");
        WriteUpgrade("Landmine",  "Alluring Scent explodes when it runs out of time");
        WriteUpgrade("Shoulder Gun", "Machine gun now has 300 ammo, but you cannot control where it shoots");
        WriteUpgrade("Companion", "You have a companion that follows your mousel, but you cannot shoot regular bullets.");
        WriteUpgrade("Shotgun", "Your regular attack now shoots 5 bullets out, expending only 3 bullets.");

        Buttons.infobtn = transform.GetChild(6).gameObject.GetComponent<Button>();
        Buttons.infotxt = Buttons.infobtn.transform.GetChild(0).GetComponent<Text>();

        if (!PlayerPrefs.HasKey("ability0"))
            PlayerPrefs.SetString("ability0", "Fire Circle");
        if (!PlayerPrefs.HasKey("ability0_key"))
            PlayerPrefs.SetString("ability0_key", "Q");
        if (!PlayerPrefs.HasKey("ability1"))
            PlayerPrefs.SetString("ability1", "Rocket Launcher"); 
        if (!PlayerPrefs.HasKey("ability1_key"))
            PlayerPrefs.SetString("ability1_key", "C"); 
        if (!PlayerPrefs.HasKey("ability2"))
            PlayerPrefs.SetString("ability2", "Shotgun"); 
        if (!PlayerPrefs.HasKey("ability2_key"))
            PlayerPrefs.SetString("ability2_key", "R"); 
        if (!PlayerPrefs.HasKey("ability3"))
            PlayerPrefs.SetString("ability3", "Machine Gun"); 
        if (!PlayerPrefs.HasKey("ability3_key"))
            PlayerPrefs.SetString("ability3_key", "F"); 
        if (!PlayerPrefs.HasKey("ability4"))
            PlayerPrefs.SetString("ability4", "Ghost"); 
        if (!PlayerPrefs.HasKey("ability4_key"))
            PlayerPrefs.SetString("ability4_key", "X"); 
        if (!PlayerPrefs.HasKey("ability5"))
            PlayerPrefs.SetString("ability5", "Alluring Scent"); 
        if (!PlayerPrefs.HasKey("ability5_key"))
            PlayerPrefs.SetString("ability5_key", "E");
        if (!PlayerPrefs.HasKey("upgrade"))
            PlayerPrefs.SetString("upgrade", "Shotgun");
        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetString("Music", "False");
        if (!PlayerPrefs.HasKey("MusicVolume"))
            PlayerPrefs.SetFloat("MusicVolume", 0);

        name_in.text = PlayerPrefs.GetString("name");
        StartCoroutine(GetWorlScore());
        if (File.Exists(Application.dataPath + "\\save.k"))
        { 
            string re = new Regex("score = [0-9]+").Match(File.ReadAllText(Application.dataPath + "\\save.k")).Value;
            highscore.text = "Best Killcount: " + new Regex("[0-9]+").Match(re).Value;
        }
        helpmenu.SetActive(false);
        play.onClick.AddListener(Play);
        help.onClick.AddListener(Help);
        close.onClick.AddListener(Close);
        name_in.onValueChanged.AddListener(NameChange);
        infobtn.onClick.AddListener(() => { infobtn.gameObject.SetActive(false); });
        infobtn.gameObject.SetActive(false);
        GameObject abi = GameObject.Find("Abilities");
        for(int i = 0; i < 6; i++)
            abi.transform.GetChild(i).gameObject.AddComponent<AbilitySelector>().index = i;
        abi.transform.GetChild(6).gameObject.AddComponent<UpgradeSelector>();
    }
    private void NameChange(string args)
    {
        if (Regex.Match(args, "[a-zA-Z]+").Value == args)
        {
            PlayerPrefs.SetString("name", args);
        }
        else
        {
            name_in.text = PlayerPrefs.GetString("name");
        }
    }
    private void Play()
    {
        if(PlayerPrefs.GetString("name").Length > 0 && !(CheckForDuplicateAbilities())) SceneManager.LoadScene(1);
        bool CheckForDuplicateAbilities()
        {
            string[] abilities = new string[6];

            for (int i = 0; i < 6; i++)
            {
                abilities[i] = PlayerPrefs.GetString("ability" + i);
            }

            return ContainsDuplicates(abilities);
        }
        bool ContainsDuplicates(string[] arr)
        {
            return arr.GroupBy(x => x).Any(g => g.Count() > 1);
        }
    }
    private void Help()
    {
        helpmenu.SetActive(true);
    }
    private void Close()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class UpgradeSelector : MonoBehaviour
{
    private Dropdown options;
    private Image sprite;
    public int index;
    private int findindex(string s)
    {
        for (int i = 0; i < options.options.Count; i++)
            if (options.options[i].text == s)
                return i;
        return 0;
    }
    public void Start()
    {
        options = gameObject.transform.GetChild(0).gameObject.GetComponent<Dropdown>();
        sprite = gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>();

        List<Dropdown.OptionData> opt = new List<Dropdown.OptionData>();
        for (int i = 0; i < Buttons.upgrades.Length; i++)
        {
            opt.Add(new Dropdown.OptionData(Buttons.upgrades[i], Resources.Load<Sprite>("upgrades/" + Buttons.upgrades[i])));
        }
        options.options = opt;

        options.value = findindex(PlayerPrefs.GetString("upgrade"));
        //options.gameObject.GetComponent<ScrollRect>().scrollSensitivity = 20f;

        sprite.sprite = Resources.Load<Sprite>("upgrades/" + options.options[options.value].text);

        sprite.gameObject.GetComponent<Button>().onClick.AddListener(() => {
            Buttons.infobtn.gameObject.SetActive(true);
            Buttons.infotxt.text = Buttons.upgrade_descriptions[options.options[options.value].text];
        });
        options.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetString("upgrade", options.options[options.value].text);
            sprite.sprite = Resources.Load<Sprite>("upgrades/" + options.options[options.value].text);
        });
    }
}
public class AbilitySelector : MonoBehaviour
{
    private Dropdown options;
    private Image sprite;
    private Text key;
    public int index;
    private int findindex(string s)
    {
        for (int i = 0; i < options.options.Count; i++)
            if (options.options[i].text == s)
                return i;
        return 0;
    }
    private void Start()
    {
        key = gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>();
        options = gameObject.transform.GetChild(1).gameObject.GetComponent<Dropdown>();
        sprite = gameObject.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Image>();

        List<Dropdown.OptionData> opt = new List<Dropdown.OptionData>();
        for (int i = 0; i < Buttons.abilities.Length; i++)
            opt.Add(new Dropdown.OptionData(Buttons.abilities[i], Resources.Load<Sprite>("Abilities/" + Buttons.abilities[i])));
        options.options = opt;

        options.value = findindex(PlayerPrefs.GetString("ability"+index));
        options.transform.GetChild(2).GetComponent<ScrollRect>().scrollSensitivity = 20f;

        sprite.sprite = Resources.Load<Sprite>("Abilities/"+options.options[options.value].text);

        key.text = PlayerPrefs.GetString("ability" + index + "_key");
        
        sprite.gameObject.GetComponent<Button>().onClick.AddListener(() => {
            Buttons.infobtn.gameObject.SetActive(true);
            Buttons.infotxt.text = Buttons.ability_descriptions[options.options[options.value].text];
        });

        key.gameObject.transform.GetComponentInParent<Button>().onClick.AddListener(Key);
        options.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetString("ability" + index, options.options[options.value].text);
            sprite.sprite = Resources.Load<Sprite>("Abilities/" + options.options[options.value].text);
        });
    }
    public void refresh_key()
    {
        key.text = PlayerPrefs.GetString("ability" + index + "_key");
    }
    void Key()
    {
        key.gameObject.transform.parent.gameObject.AddComponent<Keypress>().index = index;
    }
}

public class Keypress : MonoBehaviour
{
    public int index;
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            PlayerPrefs.SetString("ability" + index + "_key", e.keyCode.ToString());
            gameObject.transform.parent.GetComponent<AbilitySelector>().refresh_key();
            Destroy(this);
        }
    }
}