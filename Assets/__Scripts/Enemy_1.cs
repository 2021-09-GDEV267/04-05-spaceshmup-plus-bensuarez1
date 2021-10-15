using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Enemy_1 extends the Enemy class
public class Enemy_1 : Enemy {

    [Header("Set in Inspector: Enemy_1")]
    // # seconds for a full sine wave
    public float waveFrequency = 2;
    // sine wave width in meters
    public float waveWidth = 4;
    public float waveRotY = 45;

    private float x0; // The initial x value of pos
    private float birthTime;

    [Header("Set Dynamically")]
    public Text scoreGT;

    // Use this for initialization
    void Start()
    {
        GameObject scoreGO = GameObject.Find("ScoreCounter");
        scoreGT = scoreGO.GetComponent<Text>();
        scoreGT.text = "0";

        // Set x0 to the initial x position of Enemy_1
        x0 = pos.x;

        birthTime = Time.time;
    }

    // Override the Move function on Enemy
    public override void Move()
    {
        Vector3 tempPos = pos;

        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        //rotate a bit about y
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);

        // base.Move() still handles the movement down in y
        base.Move();

        // print (bndCheck.isOnScreen);
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                bool allDestroyed = true;
                if (allDestroyed) // If it IS completely destroyed...
                {
                    // ...tell the Main singleton that this ship was destroyed
                    Main.S.ShipDestroyed(this);
                    // Destroy this Enemy
                    Destroy(this.gameObject);
                    int score = int.Parse(scoreGT.text);
                    score += 100;
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
