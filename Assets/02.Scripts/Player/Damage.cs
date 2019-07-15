using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour {

    private const string bulletTag = "BULLET";
    private const string enemyTag = "ENEMY";

    
    private Color currColor;
    private readonly Color initColor = new Vector4(0, 1.0f, 0.0f, 1.0f);

    private float initHp;
    public float currHp;

    public Image bloodScreen;

    public Image hpBar;

    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    private void OnEnable()
    {
        GameManager.OnItemChange += UpdateSetup;
    }

    private void UpdateSetup()
    {
        initHp = GameManager.instance.gameData.hp;
        currHp += GameManager.instance.gameData.hp - currHp;
    }

    // Use this for initialization
    void Start () {

        initHp = GameManager.instance.gameData.hp;
        currHp = initHp;
        hpBar.color = initColor;
        currColor = initColor;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == bulletTag) {

            Destroy(coll.gameObject);

            StartCoroutine(ShowBloodScreen());

            currHp -= 5.0f;
            Debug.Log("PlayerHP = " + currHp.ToString());

            DisplayHpbar();

            if (currHp <= 0.0f) {

                PlayerDie();

            }

        }
    }

    private void PlayerDie()
    {

        OnPlayerDie();

        //Debug.Log("PlayerDie !!");

        //GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        //for (int i = 0; i < enemies.Length; i++) {

        //    enemies[i].SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);

        //}
    }

    void DisplayHpbar() {

        Debug.Log((1 - (currHp / initHp)) * 2);
        if ((currHp / initHp) > 0.5f)
            currColor.r = ((1 - (currHp / initHp)) * 2);
        else
            currColor.g = ((currHp / initHp) * 2);

        hpBar.color = currColor;
        hpBar.fillAmount = (currHp / initHp);

    }

    IEnumerator ShowBloodScreen() {

        bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.6f, 0.8f));
        yield return new WaitForSeconds(0.1f);
        bloodScreen.color = Color.clear;

    }
}
