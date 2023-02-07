using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FireCircle : FireBall
{
    public override void Start()
    {
        base.Start();
        render.color = Color.red;
    }
}
public class ShortFireBall : FireCircle
{
    public override void Update()
    {
        base.Update();
        if (timer > 0.5f)
            Destroy(gameObject);
    }
}
public class TimedFireball : FireBall
{
    public float alive;
    public override void Update()
    {
        base.Update();
        if(timer > alive)
            Destroy (gameObject);
    }
}
public class Companion : FireBall
{
    public override void Start()
    {
        base.Start();
        render.color = new Color(200, 100, 0);
    }
    public override void Update()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = transform.position.z;
        transform.up = pos - transform.position;
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "Enemy")
            {
                GameObject.Find("Player").GetComponent<CharacterController2d>().addKill();
                if (hits[i].gameObject.GetComponent<BlackMonster>() != null)
                    if (hits[i].gameObject.GetComponent<AlluringScent>() == null)
                        GameObject.Find("Player").GetComponent<CharacterController2d>().regenRandomAbility();
                Destroy(hits[i].gameObject);
            }
        }
    }
}
public class FireBall : MonoBehaviour
{
    public Rigidbody2D body;
    public SpriteRenderer render;
    public float movespeed = 500f;
    public float timer = 0;
    public float angle = 7714;
    public bool ghost = false;
    public virtual void Start()
    {
        gameObject.tag = "Helper";
        gameObject.name = "Fireball";
        gameObject.layer = 6;
        render = gameObject.AddComponent<SpriteRenderer>();
        body = gameObject.AddComponent<Rigidbody2D>();
        body.useAutoMass = false;
        body.gravityScale = 0;
        body.drag = 1;
        body.angularDrag = 1;
        body.mass = 1;
        gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 1);
        render.sprite = Resources.Load<Sprite>("Circle");
        if(angle == 7714)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = transform.position.z;
            transform.up = pos - transform.position;
        }
        else
        {
            transform.up = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            body.velocity = transform.up * movespeed / 100f;
        }
        if (!ghost) render.color = Color.magenta;
        else render.color = Color.grey;
        body.velocity = transform.up * movespeed / 100f;
    }
    public virtual void Update()
    {
        //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //pos.z = transform.position.z;
        //transform.up = pos - transform.position;
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        if (timer > 10f)
            Destroy(gameObject);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,transform.localScale.x);
        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].gameObject.tag == "Enemy")
            {
                GameObject.Find("Player").GetComponent<CharacterController2d>().addKill();
                if (hits[i].gameObject.GetComponent<BlackMonster>() != null)
                    if(hits[i].gameObject.GetComponent<AlluringScent>() == null)
                        GameObject.Find("Player").GetComponent<CharacterController2d>().regenRandomAbility();
                Destroy(hits[i].gameObject);
                Destroy(gameObject);
            }
        }
        if (ghost)
        {
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (Vector2.Distance(transform.position, ghosts[i].transform.position) <= transform.localScale.x * 2)
                {
                    GameObject.Find("Player").GetComponent<CharacterController2d>().addKill();
                    Destroy(ghosts[i]);
                    Destroy(gameObject);
                }
            }
        }
    }
}
public class Rocket : FireBall
{
    private T SpawnProjectile<T>(float angle) where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = transform.position;
        T _ = gn.AddComponent<T>();
        _.angle = angle;
        return _;
    }
    private void spawnFireCircle()
    {
        for (float i = 0f; i <= Mathf.PI * 2; i += 0.025f)
        {
            SpawnProjectile<ShortFireBall>(i);
        }
    }
    public override void Update()
    {
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        if (timer > 10f)
            Destroy(gameObject);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "Enemy")
            {
                GameObject.Find("Player").GetComponent<CharacterController2d>().addKill();
                if (hits[i].gameObject.GetComponent<BlackMonster>() != null)
                    if (hits[i].gameObject.GetComponent<AlluringScent>() == null)
                        GameObject.Find("Player").GetComponent<CharacterController2d>().regenRandomAbility();
                spawnFireCircle();
                Destroy(hits[i].gameObject);
                Destroy(gameObject);
            }
        }
    }
}
public class Monster : MonoBehaviour
{
    public Rigidbody2D body;
    public SpriteRenderer render;
    public virtual void Start()
    {
        gameObject.tag = "Enemy";
        gameObject.name = "Monster";
        gameObject.layer = 6;
        render = gameObject.AddComponent<SpriteRenderer>();
        gameObject.AddComponent<CircleCollider2D>();
        body = gameObject.AddComponent<Rigidbody2D>();
        body.useAutoMass = false;
        body.gravityScale = 0;
        body.drag = 1;
        body.angularDrag = 1;
        body.mass = 1;
    }
}
public class Wall : Monster
{
    private float timer = 0;
    public float angle = 0;
    public override void Start()
    {
        base.Start();
        gameObject.tag = "Helper";
        Destroy(gameObject.GetComponent<Rigidbody2D>());
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1,1);
        render.color = new Color(127f/255,51f/255,0f);
        render.sprite = Resources.Load<Sprite>("Square");
        transform.localScale = new Vector2(0.4f,5f);
        transform.up = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        //transform.up = GameObject.Find("Player").transform.position - transform.position;
    }
    public virtual void Update()
    {
        timer += Time.deltaTime;
        if(timer > 20f)
        {
            Destroy(gameObject);
        }
    }
}

public class House : Wall
{
    public override void Start()
    {
        base.Start();
        transform.localScale = new Vector2(3f, 3f);
    }
}
public class Turret : Monster
{
    public float timer = 0;
    public float total_time = 0;
    public override void Start()
    {
        base.Start();
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        render.color = Color.blue;
        render.sprite = Resources.Load<Sprite>("Square");
        transform.localScale = new Vector2(0.4f, 0.4f);
    }
    public virtual void Update()
    {
        timer += Time.deltaTime;
        total_time += Time.deltaTime;
        T SpawnProjectile<T>(float angle) where T : FireBall
        {
            angle %= (Mathf.PI * 2);
            GameObject gn = new GameObject();
            gn.transform.position = transform.position;
            T _ = gn.AddComponent<T>();
            _.angle = angle;
            return _;
        }
        if (timer > 1f)
        { 
            timer = 0;

            float angle = Random.Range(0f, Mathf.PI * 2);
            for (float i = -0.3f; i <= 0.3f; i += 0.1f)
                SpawnProjectile<TurretFire>(angle + i);
        }
        if(total_time > 30f)
            Destroy(gameObject);
    }
}
public class TurretFire : FireBall
{
    public override void Update()
    {
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        if (timer > 10f)
            Destroy(gameObject);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "Enemy")
            {
                if (hits[i].gameObject.GetComponent<BlackMonster>() != null)
                    if (hits[i].gameObject.GetComponent<AlluringScent>() == null)
                        GameObject.Find("Player").GetComponent<CharacterController2d>().regenRandomAbility();
                Destroy(hits[i].gameObject);
                Destroy(gameObject);
            }
        }
    }
}
public class AngryMonster : Monster
{
    public float movespeed = 100f;
    public override void Start()
    {
        base.Start();
        render.color = Color.red;
        render.sprite = Resources.Load<Sprite>("Square");
        transform.localScale = new Vector2(0.4f, 0.4f);
    }

    public virtual void Update()
    {
        transform.up = GameObject.Find("Player").transform.position - transform.position;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
    }
}
public class GhostMonster : AngryMonster
{
    private Transform playerpos;
    private float timer = 0;
    public override void Start()
    {
        base.Start();
        movespeed *= 0.5f;
        playerpos = GameObject.Find("Player").transform;
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        render.color = Color.grey;
        gameObject.tag = "Ghost";
    }
    public override void Update()
    {
        timer += Time.deltaTime;
        base.Update();
        if (Vector2.Distance(transform.position, playerpos.position) <= transform.localScale.x/2)
        {
            GameObject.Find("Player").GetComponent<CharacterController2d>().Death();
        }
        if (timer > 15f)
            Destroy(gameObject);
    }
}
public class YellowMonster : Monster
{
    private float movespeed = 100f;
    private float timer = 0;
    public override void Start()
    {
        base.Start();
        gameObject.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        render.sprite = Resources.Load<Sprite>("Square");
        render.color = Color.yellow;
        transform.up = GameObject.Find("Player").transform.position - transform.position;
    }
    public virtual void Update()
    {
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        if(timer > 10f)
            Destroy(gameObject);
    }
}
public class BlackMonster : Monster
{
    public float dragspeed = 10f; //Amount to drag the rotation of the targets towards the black hole.
    private float timer = 0;
    public float movespeed = 80f;
    public float lifetime = 25f;
    public override void Start()
    {
        base.Start();
        gameObject.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        body.mass *= 100f;
        movespeed *= 100f;
        render.sprite = Resources.Load<Sprite>("Square");
        render.color = Color.black;
        transform.up = GameObject.Find("Player").transform.position - transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        body.AddForce(transform.up * movespeed * Time.deltaTime);
        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, 5f);
        for (int i = 0; i < col.Length; i++)
        {
            if (col[i].gameObject.tag == "Enemy")
            {
                Transform tra = col[i].transform;
                tra.up = Vector3.MoveTowards(tra.up, transform.position - tra.position, dragspeed * Time.deltaTime);
            }
        }
        if (timer > lifetime)
            death();
    }

    public virtual void death()
    {
        Destroy(gameObject);
    }
}
public class AlluringScent : BlackMonster
{
    public override void Start()
    {
        base.Start();
        Destroy(gameObject.GetComponent<Collider2D>());
        movespeed = 0;
        render.color = Color.green;
    }
}
public class Landmine : AlluringScent
{
    public override void Start()
    {
        base.Start();
        lifetime = 10f;
    }
    public override void death()
    {
        spawnFireCircle();
        base.death();
    }
    private T SpawnProjectile<T>(float angle) where T : FireBall
    {
        GameObject gn = new GameObject();
        gn.transform.position = transform.position;
        T _ = gn.AddComponent<T>();
        _.angle = angle;
        return _;
    }
    private void spawnFireCircle()
    {
        for (float i = 0f; i <= Mathf.PI * 2; i += 0.025f)
        {
            SpawnProjectile<ShortFireBall>(i);
        }
    }
}
public class HatredScent : WhiteMonster
{
    public override void Start()
    {
        base.Start();
        Destroy(gameObject.GetComponent<Collider2D>());
        movespeed = 0;
        render.color = Color.red;
    }
}
public class WhiteMonster : BlackMonster
{
    public override void Start()
    {
        base.Start();
        render.color = Color.white;
        dragspeed *= -1;
    }
}