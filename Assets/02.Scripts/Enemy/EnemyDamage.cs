using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamage : MonoBehaviour {

    private const string bulletTag = "BULLET";
    private float hp = 100.0f;

    private float initHp = 100.0f;

    private GameObject bloodEffect;

    public GameObject hpBarPrefab;
    public Vector3 hpBarOffset = new Vector3(0, 2.2f, 0);

    private Canvas uiCanvas;
    private Image hpBarImage;

    // Use this for initialization
    void Start() {
        bloodEffect = Resources.Load<GameObject>("BulletImpactFleshBigEffect");
        SetHpBar();
    }

    // Update is called once per frame
    void Update() {

    }

    void SetHpBar() {

        uiCanvas = GameObject.Find("UI Canvas").GetComponent<Canvas>();
        GameObject hpBar = Instantiate<GameObject>(hpBarPrefab, uiCanvas.transform);
        hpBarImage = hpBar.GetComponentsInChildren<Image>()[1];

        var _hpbar = hpBar.GetComponent<EnemyHpBar>();
        _hpbar.targetTr = this.gameObject.transform;
        _hpbar.offset = hpBarOffset;
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == bulletTag) {

            ShowBloodEffect(coll);
            //Destroy(coll.gameObject);
            coll.gameObject.SetActive(false);

            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            hpBarImage.fillAmount = hp / initHp;

            if (hp <= 0.0f) {

                GetComponent<EnemyAI>().state = EnemyAI.State.DIE;
                hpBarImage.GetComponentsInParent<Image>()[1].color = Color.clear;

                //적 캐릭터의 사망 횟수를 누적시키는 함수 호출
                GameManager.instance.IncKillCount();
                // Capsule Collider 컴포넌트를 비활성화
                GetComponent<CapsuleCollider>().enabled = false;
            }
        }
    }

    private void ShowBloodEffect(Collision coll)
    {
        Vector3 pos = coll.contacts[0].point;
        Vector3 _normal = coll.contacts[0].normal;
        Quaternion rot = Quaternion.FromToRotation(-Vector3.forward, _normal);

        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot);
        Destroy(blood, 1.0f);
    }
}
