using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Parts
{
    // These three fields need to be defined in the Inspector pane
    public string name; // The name of this part
    public float health; // The amount of health this part has
    public string[] protectedBy; // The other parts that protect this

    // These two fields are set automatically in Start().
    // Caching like this makes it faster and easier to find these later
    [HideInInspector] // Makes field on the next line not appear in the Inspector
    public GameObject go; // The GameObject of this part
    [HideInInspector]
    public Material mat; // The Material to show damage
}

public class Enemy_5 : Enemy
{
    [Header("Set in Inspector: Enemy_5")]
    public Parts[] parts; // The array of ship Parts

    private Vector3 p0 = new Vector3(-.5f, 0, -.5f); // The two points to interpolate
    private Vector3 p1 = new Vector3(.5f, 0, .5f);
    private float timeStart; // Birth time for this Enemy_5
    private float duration = 1; // Duration of movement

    [Header("Set Dynamically")]
    public Text scoreGT;
    
    // Start is called before the first frame update
    private void Start()
    {
        GameObject scoreGO = GameObject.Find("ScoreCounter");
        scoreGT = scoreGO.GetComponent<Text>();
        scoreGT.text = "0";
        
        // There is already an initial position chosen by Main.SpawnEnemy()
        // so add it to points as the initial p0 & p1
        p0 = p1 = pos;

        InitMovement();

        // Cache GameObject & Material of each Part in parts
        Transform t;
        foreach (Parts prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        p0 = p1; // Set p0 to the old p1
        // Assign a new on-screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Reset the time
        timeStart = Time.time;
    }

    public override void Move()
    {
        // This completely overrides Enemy.Move() with a linear interpolation
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); // Apply Ease Out easing to u
        pos = ((1 - u) * p0) + (u * p1);// Simple linear interpolation
    }

    Parts FindPart(string n)
    {
        foreach (Parts prt in parts)
        {
            if (prt.name == n)
            {
                return (prt);
            }
        }
        return (null);
    }
    Parts FindPart(GameObject go)
    {
        foreach (Parts prt in parts)
        {
            if (prt.go == go)
            {
                return (prt);
            }
        }
        return (null);
    }

    // These functions return true if the Part has been destroyed
    bool Destroyed(GameObject go)
    {
        return (Destroyed(FindPart(go)));
    }
    bool Destroyed(string n)
    {
        return (Destroyed(FindPart(n)));
    }
    bool Destroyed(Parts prt)
    {
        if (prt == null) // If no real ph was passed in
        {
            return (true); // Return true (meaning yes, it was destroyed)
        }
        // Returns the result of the comparison: prt.health <= 0
        // If prt.health is 0 or less, returns true (yes, it was destroyed)
        return (prt.health <= 0);
    }

    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                // IF this Enemy is off screen, don't damage it.
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }

                // Hurt this Enemy
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Parts prtHit = FindPart(goHit);
                if (prtHit == null) // If prtHit wasn't found...
                {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                // Check whether this part is still protected
                if (prtHit.protectedBy != null)
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        // If one of the protecting parts hasn't been destroyed...
                        if (!Destroyed(s))
                        {
                            // ...then don't damage this part yet
                            Destroy(other); // Destroy the ProjectileHero
                            return; // return before damaging Enemy_4
                        }
                    }
                }

                // It's not protected, so make it take damage
                // Get the damage amount from the Projectile.type and Main.W_DEFS
                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                // Show damage on the part
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    // Instead of destroying this enemy, disable the damaged part
                    prtHit.go.SetActive(false);
                }
                // Check to see if the whole ship is destroyed
                bool allDestroyed = true; // Assume it is destroyed
                foreach (Parts prt in parts)
                {
                    if (!Destroyed(prt)) // If a part still exists...
                    {
                        allDestroyed = false; // ...change allDestroyed to false
                        break; // & break out of the foreach loop
                    }
                }
                if (allDestroyed) // If it IS completely destroyed...
                {
                    // ...tell the Main singleton that this ship was destroyed
                    Main.S.ShipDestroyed(this);
                    // Destroy this Enemy
                    Destroy(this.gameObject);
                    int score = int.Parse(scoreGT.text);
                    score += 300;
                    scoreGT.text = score.ToString();

                    if (score > HighScore.score)
                    {
                        HighScore.score = score;
                    }
                }
                Destroy(other); // Destroy the ProjectileHero
                break;
        }
    }
}
