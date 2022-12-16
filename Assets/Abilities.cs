using UnityEngine;
using UnityEngine.UI;
public class ability : MonoBehaviour
{
    public int index;
    public bool use = false;
    public string abilityName;

    public KeyCode key;
    public CharacterController2d control;

    public GameObject element;
    public Image image;
    public Text text;
    public virtual void Start()
    {
        abilityName = PlayerPrefs.GetString("ability" + index);
        key = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ability" + index + "_key"));
        element = GameObject.Find("Ability " + index);
        image = element.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>("Abilities\\" + abilityName);
        text = element.transform.GetChild(2).gameObject.GetComponent<Text>();
        text.text = key.ToString();
        control = gameObject.GetComponent<CharacterController2d>();
        element.SetActive(use);
    }
    public virtual void Update()
    {
        if(use && Input.GetKeyDown(key))
        {
            cast();
        }
    }
    public virtual void cast()
    {
        use = false;
        element.SetActive(false);
    }
    public float getMouseAngle()
    {
        return Mathf.Atan2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x
        );
    }
    public void SpawnProjectile<T>(float angle) where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = transform.position;
        gn.AddComponent<T>().angle = angle;
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
public class abilityShotgun : ability
{
    public override void cast()
    {
        base.cast();
        float angle = getMouseAngle();
        for (float i = -0.6f; i <= 0.6f; i += 0.03f)
            SpawnProjectile<FireBall>(angle + i);
    }
}
public class abilityMachineGun : ability
{
    public int ammo = 0;
    private float timer = 0;
    public override void cast()
    {
        use = false;
        ammo += (control.upgrade == "Shoulder Gun")?60:30;
        if (control.upgrade == "Back-Up Mag")
            control.giveAmmo(15);
    }
    public override void Update()
    {
        base.Update();
        timer += Time.deltaTime;
        if (timer > 0.1f && ammo > 0)
        {
            timer = 0;
            ammo--;
            text.text = ammo.ToString();
            switch (control.upgrade)
            {
                case "Shoulder Gun":
                    SpawnProjectile<FireBall>(Random.Range(0, Mathf.PI * 2));
                    break;
                case "Dual Wield":
                    for (float i = -0.2f; i <= 0.2f; i += 0.4f)
                        SpawnProjectile<FireBall>(getMouseAngle(),transform.position + (transform.right*i));
                    break;
                default:
                    SpawnProjectile<FireBall>(getMouseAngle());
                    break;
            }
            if (ammo == 0)
            {
                element.SetActive(use);
                text.text = key.ToString();
            }
        }
    }
}
public class abilityComet : ability
{
    private bool active = false;
    private float timer = 0;
    private float timer2 = 0;
    public Image mask;
    public override void Start()
    {
        base.Start();
        mask = image.transform.parent.GetComponent<Image>();
    }
    public override void cast()
    {
        use = false;
        active = true;
        gameObject.GetComponent<CharacterController2d>().immortality++;
    }
    public override void Update()
    {
        base.Update();
        if (active)
        {
            timer += Time.deltaTime;
            timer2 += Time.deltaTime;
            if (timer2 > 0.01f)
            {
                timer2 = 0;
                for (int i = 0; i < 5; i++)
                    shootTimedFireball(Random.Range(0f, Mathf.PI * 2), transform.position, 0.1f, 200f);
            }
            image.color = new Color(1, 1, 1, 1f - (timer / 5));
            mask.color = new Color(1, 1, 1, 1f - (timer / 5));
            text.text = ((int)(100 - (timer * 20))).ToString();
            if (timer > 5f)
            {
                gameObject.GetComponent<CharacterController2d>().immortality--;
                timer = 0;
                image.color = Color.white;
                mask.color = image.color;
                text.text = key.ToString();
                element.SetActive(use);
                active = false;
            }
        }
    }
}
public class abilityNecromancy : ability
{
    private int lastkill = 0;
    public override void cast()
    {
        base.cast();
        for (int i = 0; i < Mathf.Min(control.killcount - lastkill,control.upgrade == "Graveyard"?150:75); i++)
        {
            control.SpawnRandom<FireBall>(Camera.main.ScreenToWorldPoint(Input.mousePosition)).angle = 7714;
        }
        lastkill = control.killcount;
        //GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        //for(int i = 0; i < ghosts.Length; i++)
        //{
        //    for (float c = 0f; c <= Mathf.PI * 2; c += 0.1f)
        //    {
        //        SpawnProjectile<FireCircle>(c,ghosts[i].transform.position);
        //    }
        //    Destroy(ghosts[i]);
        //}
    }
}
public class abilityWall : ability
{
    public override void cast()
    {
        base.cast();
        Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(control.upgrade == "House")
        {
            SpawnMonster<house>(point).angle = (getMouseAngle() + Mathf.PI / 2);
        }
        else
        {
            SpawnMonster<Wall>(point).angle = (getMouseAngle() + Mathf.PI / 2);
            SpawnMonster<Wall>(Vector2.MoveTowards(transform.position, point, -Vector2.Distance(transform.position, point))).angle = (getMouseAngle() + Mathf.PI / 2);
        }
    }
}
public class abilityTurret : ability
{
    public override void cast()
    {
        base.cast();
        SpawnMonster<Turret>(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
}
public class abilityFireCircle : ability
{
    public override void cast()
    {
        base.cast();
        for (float i = 0f; i <= Mathf.PI * 2; i += 0.1f)
        {
            SpawnProjectile<FireCircle>(i);
        }
    }
}
public class abilityRocketLauncher : ability
{
    public override void cast()
    {
        base.cast();
        SpawnProjectile<Rocket>(getMouseAngle());
    }
}
public class abilityGhost : ability
{
    public bool active = false;
    Image mask;
    float timer = 0;
    const float alivetime = 10f;
    public override void Start()
    {
        base.Start();
        mask = image.transform.parent.GetComponent<Image>();
    }
    public override void cast()
    {
        use = false;
        active = true;
        gameObject.GetComponent<CharacterController2d>().immortality++;
    }
    public override void Update()
    {
        base.Update();
        if (active)
        {
            timer += Time.deltaTime;
            image.color = new Color(1, 1, 1, 1f - (timer / alivetime));
            mask.color = new Color(1, 1, 1, 1f - (timer / alivetime));
            text.text = ((int)(100 - (timer * (100/alivetime)))).ToString();
            if (timer > alivetime)
            {
                gameObject.GetComponent<CharacterController2d>().immortality--;
                timer = 0;
                image.color = Color.white;
                mask.color = image.color;
                text.text = key.ToString();
                element.SetActive(use);
                active = false;
            }
        }
    }
}
public class abilityAlluringScent : ability
{
    public override void cast()
    {
        base.cast();
        switch (control.upgrade) {
            case "Aura of Hatred":
                SpawnMonster<HatredScent>(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case "Landmine":
                SpawnMonster<Landmine>(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            default:    
                SpawnMonster<AlluringScent>(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
        }
    }
}
public class abilityTeleport : ability
{
    public override void cast()
    {
        base.cast();
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 position = transform.position;
        float alive = 0.5f;
        while (position != target)
        {
            position = Vector2.MoveTowards(position, target, 0.1f);
            shootTimedFireball(Random.Range(0, Mathf.PI * 2), position, alive, 100f);
        }
        transform.position = target;
    }
}
public class abilitySword : ability
{
    private bool dir = true;
    public int slashesleft;
    public override void cast()
    {
        use = false;
        slashesleft += 5;
        text.text = slashesleft.ToString();
    }
    public override void Update()
    {
        base.Update();
        if (slashesleft > 0)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                gameObject.AddComponent<Slash>().dir = dir;
                dir = !dir;
                slashesleft--;
                text.text = slashesleft.ToString();
                if (slashesleft == 0)
                {
                    text.text = key.ToString();
                    element.SetActive(use);
                }
            }
        }
    }
}
public class Slash : MonoBehaviour
{
    public bool dir;
    public float startangle;
    private float angle;
    void shootTimedFireball(float angle, Vector2 start, float alive, float movespeed)
    {
        GameObject gn = new GameObject();
        gn.transform.position = start;
        TimedFireball _ = gn.AddComponent<TimedFireball>();
        _.angle = angle;
        _.alive = alive;
        _.movespeed = movespeed;
    }
    private void Start()
    {
        Camera cam = Camera.main;
        startangle += Mathf.Atan2(
            cam.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y,
            cam.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x
        ) + (dir ? -2 : 2);
        angle = startangle;
    }
    private void Update()
    {
        shootTimedFireball(angle, transform.position, 0.15f, 1000f);
        angle += dir ? 0.1f : -0.1f;
        if (dir) { 
            if(angle > (startangle + 4))
            {
                Destroy(this);
            }
        }
        else
        {
            if(angle < (startangle - 4))
            {
                Destroy(this);
            }
        }
    }
}