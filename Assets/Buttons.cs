using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [SerializeField] Button play;
    [SerializeField] Button help;
    [SerializeField] Button close;
    [SerializeField] InputField name_in;
    [SerializeField] GameObject helpmenu;
    [SerializeField] Text highscore;
    [SerializeField] Text worldhighscore;
    public static string[] abilities = { "Shotgun", "Necromancy", "Wall", "Turret", "Machine Gun", "Comet", "Fire Circle", "Rocket Launcher", "Ghost", "Alluring Scent", "Teleport", "Slash" };
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
    void Start()
    {
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
        GameObject abi = GameObject.Find("Abilities");
        for(int i = 0; i < 6; i++)
            abi.transform.GetChild(i).gameObject.AddComponent<AbilitySelector>().index = i;
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
        if(PlayerPrefs.GetString("name").Length > 0) SceneManager.LoadScene(1);
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