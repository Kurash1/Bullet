using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class CharacterController2d : MonoBehaviour
{
    //Variables
    private float MoveSpeed;
    public int killcount = 0;
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
    //Constants
    [SerializeField] private float BaseMoveSpeed;
    //Resources
    int FireballAmount = 0;
    //Abilities
    public string upgrade = "Aura of Hatred"; //Back-Up Mag //Shoulder Gun //Dual Wield //Aura of Hatred //Landmine
    public ability[] abilities = new ability[6];
    public byte immortality = 0;
    public void giveAmmo(int amount) { FireballAmount += amount; BulletField.text = FireballAmount.ToString(); }
    void Start()
    {
        upgrade = PlayerPrefs.GetString("upgrade");
        Camera.main.gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("upgrades/" + upgrade);
        void a<T>(int i) where T : ability { abilities[i] = gameObject.AddComponent<T>(); abilities[i].index = i; }
        for(int i = 0; i < 6; i++)
        {
            switch (PlayerPrefs.GetString("ability" + i))
            {
                case "Shotgun": a<abilityShotgun>(i); break;
                case "Machine Gun": a<abilityMachineGun>(i); break;
                case "Comet": a<abilityComet>(i); break;
                case "Necromancy": a<abilityNecromancy>(i); break;
                case "Wall": a<abilityWall>(i); break;
                case "Turret": a<abilityTurret>(i); break;
                case "Fire Circle": a<abilityFireCircle>(i); break;
                case "Rocket Launcher": a<abilityRocketLauncher>(i); break;
                case "Ghost": a<abilityGhost>(i); break;
                case "Alluring Scent": a<abilityAlluringScent>(i); break;
                case "Teleport": a<abilityTeleport>(i); break;
                case "Slash": a<abilitySword>(i); break;
            }
        }

        MoveSpeed = BaseMoveSpeed;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void regenRandomAbility()
    {
        //Local Functions
        bool AnyFalse(ability[] bools)
        {
            for (int i = 0; i < bools.Length; i++)
                if (!bools[i].use)
                    return true;
            return false;
        }
        void regenAbility(int index)
        {
            abilities[index].element.SetActive(true);
            abilities[index].use = true;
        }
        //Actual Code
        if (AnyFalse(abilities))
        {
            int rand = Random.Range(0, abilities.Length);
            while (abilities[rand].use)
            {
                rand = Random.Range(0, abilities.Length);
            }
            regenAbility(rand);
        }
    }
    void Update()
    {
        secondtimer += Time.deltaTime;
        timemult = Mathf.Max(Mathf.Pow(secondtimer%30+1,3)/1000f,1) + secondtimer/30;
        time.text =
        "Time (seconds)\n" +
        (int)(secondtimer) +
        "\nSpeed\n" +
        (int)(timemult * 100) + "%";
        //Vector2 toVector2(Vector3 a) { return new Vector2(a.x, a.y); }
        //transform.up = toVector2(Camera.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        Vector2 MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
        float AngleRad = Mathf.Atan2(MousePos.y - transform.position.y, MousePos.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        //body.rotation = AngleDeg - 90;
        //body.MoveRotation(Quaternion.LookRotation(body.position - MousePos));
        body.MoveRotation(AngleDeg - 90);

        bool vector2inrange(float min_x, float max_x, float min_y, float max_y, Vector2 value) {
            return (value.x >= min_x) && (value.x <= max_x) && (value.y >= min_y) && (value.y <= max_y); }
        arrow.up = -transform.position; 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(0);
        }
        HandleMovement();
        tryFireBall();
        HandleCamera();
        timer += Time.deltaTime;
        if (timer > (1f / timemult))
        {
            scor.text = "Killcount: " + killcount;
            timer = 0;
            SpawnMonster<YellowMonster>();
            if (vector2inrange(-5.12f, 5.12f, -5.12f, 5.12f, transform.position))
            {
                arrow.localPosition = new Vector3(0, 0, 2);
                score += 1;
                if(score % 10 == 0)
                {
                    SpawnMonster<AngryMonster>();
                }
                if(score % 5 == 0)
                {
                    FireballAmount++;
                    BulletField.text = FireballAmount.ToString();
                }
                if(score % 25 == 0)
                {
                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            SpawnMonster<BlackMonster>();
                            break;
                        case 1:
                            SpawnMonster<WhiteMonster>();
                            break;
                    }
                }
                if(score % 100 == 0)
                {
                    SpawnMonster<GhostMonster>();
                }
            }
            else
                arrow.localPosition = new Vector3(0, 0, -1);
        }
        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
        for(int i = 0; i < col.Length; i++)
            if (col[i].tag == "Enemy" && immortality <= 0)//!(ghost|comet) && col[i].tag == "Enemy")
                Death();
    }
    public void Death()
    {
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
        //Code
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
        SceneManager.LoadScene(1);
        gameObject.SetActive(false);
    }
    void HandleCamera()
    {
        Camera.transform.position = new Vector3(Mathf.Lerp(Camera.transform.position.x, transform.position.x, 0.05f), Mathf.Lerp(Camera.transform.position.y,transform.position.y,0.05f), -10);
    }
    void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            regenRandomAbility();
        }
        if (Input.GetKey(KeyCode.LeftShift))//|comet) 
            MoveSpeed = BaseMoveSpeed * 2;
        else
            MoveSpeed = BaseMoveSpeed;
        body.velocity = Vector2.ClampMagnitude(new Vector2(
            (Input.GetAxis("Horizontal") * MoveSpeed),
            (Input.GetAxis("Vertical")   * MoveSpeed)
        ),MoveSpeed);
    }
    public void tryFireBall()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && FireballAmount > 0)
        {
            FireballAmount--;
            BulletField.text = FireballAmount.ToString();
            SpawnProjectile<FireBall>().ghost = upgrade == "Ghost Hunter" || gameObject.GetComponent<abilityGhost>().active;
        }
    }
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
}
