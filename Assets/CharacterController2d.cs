using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class CharacterController2d : MonoBehaviour
{
    //Variables
    private float MoveSpeed;
    private int killcount = 0;
    private float score = 0;
    private double timer = 0;
    //Objects
    [SerializeField] private Text scor;
    [SerializeField] Camera Camera;
    [SerializeField] Text BulletField;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform arrow;
    [SerializeField] private Text time;
    private float secondtimer = 1;
    private float timemult = 1;
    [SerializeField] private GameObject pausemenu;
    //Constants
    [SerializeField] public float BaseMoveSpeed;
    float DefaultTimeRate = 1.0f;
    //Resources
    private int FireballAmount = 0;
    //Abilities
    public string upgrade = "Aura of Hatred"; //Back-Up Mag //Shoulder Gun //Dual Wield //Aura of Hatred //Landmine
    public Dictionary<string,ability> abilities = new Dictionary<string, ability>();
    public byte immortality = 0;
    public void addKill(int amount = 1) { killcount += amount; scor.text = "Killcount: " + killcount.ToString(); }
    public int getKillCount() { return killcount; }
    public void giveAmmo(int amount) {
        if (upgrade == "Companion") return;
        FireballAmount += amount;
        BulletField.text = FireballAmount.ToString(); 
    }
    public int getAmmo() { return FireballAmount; }
    public void SetSpeed(float speed)
    {
        BaseMoveSpeed = speed;
        MoveSpeed = speed;
    }
    public void SetTimeRate(float timerate)
    {
        DefaultTimeRate = timerate;
        Time.timeScale = timerate;
    }
    void Start()
    {
        upgrade = PlayerPrefs.GetString("upgrade");
        if(upgrade == "Companion")
        {
            SpawnProjectile<Companion>();
        }
        Camera.main.gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("upgrades/" + upgrade);
        void a<T>(string s, int i) where T : ability { abilities.Add(s,gameObject.AddComponent<T>()); abilities[s].index = i; }
        for(int i = 0; i < 6; i++)
        {
            switch (PlayerPrefs.GetString("ability" + i))
            {
                case "Shotgun": a<abilityShotgun>("Shotgun",i); break;
                case "Machine Gun": a<abilityMachineGun>("Machine Gun",i); break;
                case "Comet": a<abilityComet>("Comet",i); break;
                case "Necromancy": a<abilityNecromancy>("Necromancy",i); break;
                case "Wall": a<abilityWall>("Wall",i); break;
                case "Turret": a<abilityTurret>("Turret",i); break;
                case "Fire Circle": a<abilityFireCircle>("Fire Circle",i); break;
                case "Rocket Launcher": a<abilityRocketLauncher>("Rocket Launcher",i); break;
                case "Ghost": a<abilityGhost>("Ghost",i); break;
                case "Alluring Scent": a<abilityAlluringScent>("Alluring Scent",i); break;
                case "Teleport": a<abilityTeleport>("Teleport",i); break;
                case "Slash": a<abilitySword>("Slash",i); break;
                case "Slow Time": a<abilityTimeSlow>("Slow Time",i); break;
            }
        }

        MoveSpeed = BaseMoveSpeed;
        Cursor.lockState = CursorLockMode.Confined;
        regenRandomAbility();
        giveAmmo(10);
    }
    void Update()
    {
        timer += Time.deltaTime;
        doTimeMult();
        doMovement();
        doCollisions();
        handleFireball();
        SpawnEnemies();
        doRotation();
        doCamera();
        handleInputs();
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.V))
        {
            regenRandomAbility();
            giveAmmo(10);
        }
#endif
    }
    public void regenRandomAbility()
    {
        //Local Functions
        bool AnyFalse(Dictionary<string,ability> bools)
        {
            for (int i = 0; i < bools.Count; i++)
                if (!bools.ElementAt(i).Value.use)
                    return true;
            return false;
        }
        void regenAbility(int index)
        {
            abilities.ElementAt(index).Value.element.SetActive(true);
            abilities.ElementAt(index).Value.use = true;
        }
        //Actual Code
        if (AnyFalse(abilities))
        {
            int rand = Random.Range(0, abilities.Count);
            while (abilities.ElementAt(rand).Value.use)
            {
                rand = Random.Range(0, abilities.Count);
            }
            regenAbility(rand);
        }
    }
    void doTimeMult()
    {
        secondtimer += Time.deltaTime;
        timemult = Mathf.Max(Mathf.Pow(secondtimer % 30 + 1, 3) / 1000f, 1) + secondtimer / 30;
        time.text =
        "Time (seconds)\n" +
        (int)(secondtimer) +
        "\nSpeed\n" +
        (int)(timemult * 100) + "%";
    }
    void doRotation()
    {
        Vector2 MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
        float AngleRad = Mathf.Atan2(MousePos.y - transform.position.y, MousePos.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        body.MoveRotation(AngleDeg - 90);

        arrow.up = -transform.position; 
    }
    public void Pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pausemenu.SetActive(true);
        pausemenu.transform.GetChild(1).GetComponent<Text>().text = "Kill Count: " + killcount + "\nTime: " + (int)secondtimer;
        //SceneManager.LoadScene(0);
    }
    public void Unpause()
    {
        Time.timeScale = DefaultTimeRate;
        pausemenu.SetActive(false);
    }
    void SpawnEnemies()
    {
        if (timer > (1f / timemult))
        {
            timer = 0;
            SpawnMonster<YellowMonster>();
            if (vector2inrange(-5.12f, 5.12f, -5.12f, 5.12f, transform.position))
            {
                arrow.localPosition = new Vector3(0, 0, 2);
                score += 1;
                if (score % 10 == 0)
                    SpawnMonster<AngryMonster>();
                if (score % 5 == 0)
                {
                    giveAmmo(1);
                }
                if (score % 100 == 0)
                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            SpawnMonster<BlackMonster>();
                            break;
                        case 1:
                            SpawnMonster<WhiteMonster>();
                            break;
                    }
                if (score % 100 == 0)
                    SpawnMonster<GhostMonster>();
                if (score % 250 == 0)
                    SpawnMonster<RangedMonster>();
            }
            else
                arrow.localPosition = new Vector3(0, 0, -1);
        }
        bool vector2inrange(float min_x, float max_x, float min_y, float max_y, Vector2 value)
        {
            return (value.x >= min_x) && (value.x <= max_x) && (value.y >= min_y) && (value.y <= max_y);
        }
    }
    void doCollisions()
    {
        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
        for (int i = 0; i < col.Length; i++)
            if (col[i].tag == "Enemy" && immortality <= 0)
                Death();
    }
    void handleInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            if(Time.timeScale == 0)
                Unpause();
            else
                Pause();
    }
    public void Death()
    {
        StartCoroutine(GetWorlScore());
        if (!File.Exists(Application.dataPath + "\\save.k"))
        {
            File.Create(Application.dataPath + "\\save.k").Close();
            File.WriteAllText(Application.dataPath + "\\save.k", "score = 0");
        }
        string[] savefile = File.ReadAllLines(Application.dataPath + "\\save.k");
        for (int i = 0; i < savefile.Length; i++)
        {
            if(Regex.IsMatch(savefile[i],"score = [0-9]+"))
            {
                int oldscore = int.Parse(new Regex("[0-9]+").Match(savefile[i]).Value);
                if(killcount > oldscore)
                {
                    savefile[i] = "score = " + killcount;
                }
            }
        }
        File.WriteAllLines(Application.dataPath + "\\save.k", savefile);
        SetTimeRate(1);
        SceneManager.LoadScene(1);
        //gameObject.SetActive(false);
        //Local Function
        IEnumerator GetWorlScore()
        {
            string uri = "http://jesser.vlab.fi/PHP/bullet.php";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri + "?score=" + killcount + "&name=" + PlayerPrefs.GetString("name")))
            {
                //Debug.Log(uri + "?score=" + score);
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
                        break;
                }
            }
        }
    }
    void doCamera()
    {
        Camera.transform.position = new Vector3(Mathf.Lerp(Camera.transform.position.x, transform.position.x, 0.05f), Mathf.Lerp(Camera.transform.position.y,transform.position.y,0.05f), -10);
    }
    void doMovement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            MoveSpeed = BaseMoveSpeed * 2;
        else
            MoveSpeed = BaseMoveSpeed;
        body.velocity = Vector2.ClampMagnitude(new Vector2(
            (Input.GetAxis("Horizontal") * MoveSpeed),
            (Input.GetAxis("Vertical")   * MoveSpeed)
        ),MoveSpeed);
    }
    public void handleFireball()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && FireballAmount > 0)
        {
            if(upgrade == "Shotgun" && FireballAmount >= 3) {
                FireballAmount -= 3;
                float angle = getMouseAngle();
                for (float i = -0.3f; i <= 0.3f; i += 0.15f)
                    SpawnProjectile<FireBall>(angle + i).ghost = upgrade == "Ghost Hunter" || abilities.ContainsKey("Ghost") && gameObject.GetComponent<abilityGhost>().active;
            }
            else
            {
                FireballAmount--;
                SpawnProjectile<FireBall>().ghost = upgrade == "Ghost Hunter" || abilities.ContainsKey("Ghost") && gameObject.GetComponent<abilityGhost>().active;
            }
            BulletField.text = FireballAmount.ToString();
        }
    }
    public float getMouseAngle()
    {
        return Mathf.Atan2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x
        );
    }
    //Spawn Functions
    public T SpawnRandom<T>(Vector2 pos) where T : Component
    {
        GameObject gn = new GameObject();
        float angle = Random.Range(0f, 360f);
        gn.transform.position = Vector2.MoveTowards(pos, new Vector2(pos.x + (Mathf.Cos(angle) * 10), pos.y + (Mathf.Sin(angle) * 10)), Random.Range(5f, 10f));
        return gn.AddComponent<T>();
    }
    public T SpawnMonster<T>() where T : Component
    {
        GameObject gn = new GameObject();
        float angle = Random.Range(0f, 360f);
        gn.transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x + (Mathf.Cos(angle) * 10), transform.position.y + (Mathf.Sin(angle) * 10)), Random.Range(5f, 10f));
        return gn.AddComponent<T>();
    }
    public T SpawnProjectile<T>() where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = transform.position;
        return gn.AddComponent<T>();
    }

    public T SpawnProjectile<T>(float angle) where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = transform.position;
        T a = gn.AddComponent<T>();
        a.angle = angle;
        return a;
    }
    public T SpawnProjectile<T>(float angle, Vector2 pos) where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = pos;
        T _ = gn.AddComponent<T>();
        _.angle = angle;
        return _;
    }
    public void shootTimedFireball(float angle, Vector2 start, float alive, float movespeed)
    {
        GameObject gn = new GameObject();
        gn.transform.position = start;
        TimedFireball _ = gn.AddComponent<TimedFireball>();
        _.angle = angle;
        _.alive = alive;
        _.movespeed = movespeed;
    }
    public T SpawnMonster<T>(Vector2 pos) where T : Component
    {
        GameObject gn = new GameObject();
        gn.transform.position = pos;
        return gn.AddComponent<T>();
    }
}
