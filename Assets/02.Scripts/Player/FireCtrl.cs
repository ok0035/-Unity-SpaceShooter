using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public struct PlayerSfx
{
    public AudioClip[] fire;
    public AudioClip[] reload;
}

public class FireCtrl : MonoBehaviour {

    //무기타입
    public enum WeaponType {

        RIFLE = 0,
        SHOTGUN
    }

    public WeaponType currWeapon = WeaponType.RIFLE;
    public GameObject bullet;
    public Transform firePos;
    public ParticleSystem cartridge;
    public PlayerSfx playerSfx;

    public Image magazineImg;
    public Text magazineText;

    public int maxBullet = 10;
    public int remainingBullet = 10;

    public float reloadTime = 2.0f;
    private bool isReloading = false;

    private ParticleSystem muzzleFlash;
    private AudioSource _audio;

    private Shake shake;

    //변경할 무기 이미지
    public Sprite[] weaponIcons;
    //교체할 무기 이미지UI
    public Image weaponImage;

    private int enemyLayer; // 적 캐릭터의 레이어 값을 저장할 변수
    private bool isFire = false; // 자동 발사 여부를 판단할 변수
    private float nextFire; // 다음 발사 시간을 저장할 변수
    private float fireRate = 0.1f; // 총알의 발사간격

    //장애물의 레이어 값을 저장할 변수
    private int obstacleLayer;
    //레이어 마스크의 비트 연산을 위한 변수
    private int layerMask;
    //드럼통 레이어 값을 저장할 변수
    private int barrelLayer;
   

	// Use this for initialization
	void Start () {
        //하위에 있는 컴포넌트 추출
        muzzleFlash = firePos.GetComponentInChildren<ParticleSystem>();

        //AudioSorce 컴포넌트 추출
        _audio = GetComponent<AudioSource>();

        shake = GameObject.Find("CameraRig").GetComponent<Shake>();

        //적 캐릭터의 레이어 값을 추출
        enemyLayer = LayerMask.NameToLayer("ENEMY");

        //장애물의 레이어 값을 추출
        obstacleLayer = LayerMask.NameToLayer("OBSTACLE");

        //드럼통 레이어 값을 추출
        barrelLayer = LayerMask.NameToLayer("BARREL");

        layerMask = 1 << obstacleLayer | 1 << enemyLayer | 1 << barrelLayer;
	}
	
	// Update is called once per frame
	void Update () {

        Debug.DrawRay(firePos.position, firePos.forward * 20.0f, Color.green);

        if (EventSystem.current.IsPointerOverGameObject()) return;

        //레이캐스트에 검출된 객체의 정보를 저장할 변수
        RaycastHit hit;

        //레이캐스트를 생성해 적 캐릭터를 검출
        if (Physics.Raycast(firePos.position, //광선의 발사 원점 좌표
                            firePos.forward, //광선의 발사 방향 벡터
                            out hit, //검출된 객체의 정보를 반환받을 변수
                            20.0f, //광선의 도달 거리
                            layerMask)) // 검출할 레이어
            //isFire = (hit.collider.CompareTag("ENEMY"));
            isFire = false;
        else
            isFire = false;

        //레이캐스트에 적 캐릭터가 닿았을 때 자동발사
        if(!isReloading && isFire)
        {
            if(Time.time > nextFire)
            {
                //총알 수를 하나 감소
                --remainingBullet;
                Fire();

                //남은 총알이 없을 경우 재장전 코루틴 호출
                if(remainingBullet == 0)
                {
                    StartCoroutine(Reloading());
                }
                nextFire = Time.time + fireRate;
            }
        }

        //마우스 왼쪽 버튼을 클릭했을 떄 Fire함수 호출
        if (!isReloading && Input.GetMouseButtonDown(0)) {

            --remainingBullet;
            Fire();

            //남은 총알이 없을 경우 재장전 코루틴 호출
            if (remainingBullet == 0) {

                StartCoroutine(Reloading());

            }

        }
	}

    IEnumerator Reloading()
    {
        isReloading = true;
        _audio.PlayOneShot(playerSfx.reload[(int)currWeapon], 1.0f);

        yield return new WaitForSeconds(playerSfx.reload[(int)currWeapon].length + 0.3f);

        isReloading = false;
        magazineImg.fillAmount = 1.0f;
        remainingBullet = maxBullet;
        UpdateBulletText();

    }

    public void OnChangeWeapon()
    {
        currWeapon = (WeaponType)((int)++currWeapon % 2);
        weaponImage.sprite = weaponIcons[(int)currWeapon];
    }

    void Fire()
    {
        StartCoroutine(shake.ShakeCamera());
        //Instantiate(bullet, firePos.position, firePos.rotation);

        var _bullet = GameManager.instance.GetBullet();
        if(_bullet != null)
        {
            _bullet.transform.position = firePos.position;
            _bullet.transform.rotation = firePos.rotation;
            _bullet.SetActive(true);
        }

        cartridge.Play();
        muzzleFlash.Play();
        FireSfx();

        magazineImg.fillAmount = (float)remainingBullet / (float)maxBullet;
        UpdateBulletText();
        /*
         * 
         *Instantiate<GameObject>(bullet, firePos.position, firePos.rotation);
         *Instantiate<GameObject>(bullet, firePos.position, firePos.rotation, null);
         *Instantiate<GameObject>(bullet, firePos);
         *Instantiate<GameObject>(bullet, firePos, false);
         *
         * 
         */
    }

    private void UpdateBulletText()
    {
        magazineText.text = string.Format("<color=#ff0000>{0}</color>{1}", remainingBullet, maxBullet);
    }

    private void FireSfx()
    {
        var _sfx = playerSfx.fire[(int)currWeapon];
        _audio.PlayOneShot(_sfx, 1.0f);
    }
}
