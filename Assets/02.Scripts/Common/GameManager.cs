using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataInfo;

public class GameManager : MonoBehaviour {

    [Header("Enemy Create Info")]
    //적 캐릭터가 출현할 위치를 담을 배열
    public Transform[] points;
    //적 캐릭터 프리팹을 저장할 변수
    public GameObject enemy;
    //적 캐릭터를 생성할 주기
    public float createTime = 2.0f;
    //적 캐릭터의 최대 생성 개수
    public int maxEnemy = 10;
    //게임 종료 여부를 판단할 변수
    public bool isGameOver = false;

    public CanvasGroup inventoryCG;

    public static GameManager instance = null;

    [Header("Object Pool")]
    public GameObject bulletPrefab;
    public int maxPool = 10;
    public List<GameObject> bulletPool = new List<GameObject>();

    //일시정지 구현
    private bool isPaused;

    //PlayerPrefs를 활용한 데이터 저장
    [HideInInspector] public int killCount;
    //적 캐릭터를 죽인 횟수를 표시할 텍스트 UI
    public Text killCountTxt;

    //DataManager를 저장할 변수
    private DataManager dataManager;
    //public GameData gameData;

    public GameDataObject gameData;

    //인벤토리의 아이템이 변경됐을 떄 발생시킬 이벤트 정의
    public delegate void ItemChangeDelegate();
    public static event ItemChangeDelegate OnItemChange;

    //SlotList 게임오브젝트를 저장할 변수
    private GameObject slotList;
    //ItemList 하위에 있는 네 개의 아이템을 저장할 배열
    public GameObject[] itemObjects;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        } else if(instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        //DataManager를 추출
        dataManager = GetComponent<DataManager>();
        //DataManager 초기화
        dataManager.Initialize();

        //인벤토리에 추가된 아이템을 검색하기 위해서 SlotList 게임 오브젝트를 추출
        slotList = inventoryCG.transform.Find("SlotList").gameObject;

        //게임의 초기 데이터 로드
        LoadGameData();

        CreatePooling();
    }

    // Use this for initialization
    void Start () {

        OnInventoryOpen(false);

        points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();

        if(points.Length > 0)
        {
            StartCoroutine(this.CreateEnemy());
        }
	}

    //앱이 종료되는 시점에 호출되는 이벤트 함수
    private void OnApplicationQuit()
    {
        //게임 종료 전 게임 데이터를 저장한다.
        SaveGameData();
    }

    public void AddItem(Item item)
    {
        //보유 아이템에 같은 아이템이 있으면 추가하지 않고 빠져나감
        if (gameData.equipItem.Contains(item)) return;

        //아이템을  GameData.equipItem 배열에 추가
        gameData.equipItem.Add(item);

        //아이템의 종류에 따라 분기처리
        switch(item.itemType)
        {
            case Item.ItemType.HP:
                //아이템의 계산 방식에 따라 연산처리
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.hp += item.value;
                else
                    gameData.hp += gameData.hp * item.value;
                break;

            case Item.ItemType.DAMAGE:
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.damage += item.value;
                else
                    gameData.damage += gameData.damage * item.value;
                break;

            case Item.ItemType.SPEED:
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.speed += item.value;
                else
                    gameData.speed += gameData.speed * item.value;
                break;

            case Item.ItemType.GRENADE:
                break;
        }

        UnityEditor.EditorUtility.SetDirty(gameData);

        //아이템이 변경된 것을 실시간으로 반영하기 위해 이벤트를 발생시킴
        OnItemChange();
        
    }


    //인벤토리에서 아이템을 제거했을 때 데이터를 갱신하는 함수
    public void RemoveItem(Item item)
    {
        //아이템을 GameData.equipItem 배열에서 삭제
        gameData.equipItem.Remove(item);

        //아이템의 종류에 따라 분기처리
        switch (item.itemType)
        {

            /*
             예를 들어서
             스피드가 6
             스피드 아이템 +
             (6 * 0.1) + 6 = 6.6
             (x * value) + x = 6.6;
             x(1 + value) = 6.6;
             x = gameData.speed / (1 + value);
             */

            case Item.ItemType.HP:
                //아이템의 계산 방식에 따라 연산처리
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.hp -= item.value;
                else
                    gameData.hp = gameData.hp / (1.0f + item.value);
                break;

            case Item.ItemType.DAMAGE:
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.damage -= item.value;
                else
                    gameData.damage = gameData.damage / (1.0f + item.value);
                break;

            case Item.ItemType.SPEED:
                if (item.itemCalc == Item.ItemCalc.INC_VALUE)
                    gameData.speed -= item.value;
                else
                    gameData.speed = gameData.speed / (1.0f + item.value);
                break;

            case Item.ItemType.GRENADE:
                break;
        }

        //.asset파일에 데이터 저장
        UnityEditor.EditorUtility.SetDirty(gameData);

        //아이템이 변경된 것을 실시간으로 반영하기 위해 이벤트를 발생시킴
        OnItemChange();
    }

    void LoadGameData()
    {

        /*
        * ScriptableObject는 전역적으로 접근이 가능해서 
        * 별도로 로드하는 과정이 필요 없다.
        */

        //DataManager를 통해 파일에 저장된 데이터 불러오기

        //GameData data = dataManager.Load();

        //gameData.hp = data.hp;
        //gameData.damage = data.damage;
        //gameData.speed = data.speed;
        //gameData.killCount = data.killCount;
        //gameData.equipItem = data.equipItem;

        //보유한 아이템이 있을때만 호출
        if (gameData.equipItem.Count > 0)
        {
            InventorySetup();
        }

        //KILL_COUNT 키로 저장된 값을 로드한다.
        //killCount = PlayerPrefs.GetInt("KILL_COUNT", 0);
        killCountTxt.text = "KILL " + gameData.killCount.ToString("0000");
    }

    private void InventorySetup()
    {
        //SlotList 하위에 있는 모든 Slot을 추출
        var slots = slotList.GetComponentsInChildren<Transform>();

        //보유하고있는 아이템의 개수만큼 반복
        for(int i=0; i<gameData.equipItem.Count; i++)
        {
            //인벤토리 UI에 있는 Slot 개수만큼 반복
            for(int j=1; j<slots.Length; j++)
            {
                //Slot 하위에 다른 아이템이 있으면 다음 인덱스로 넘어감
                if (slots[j].childCount > 0) continue;

                //보유한 아이템의 종류에 따라서 인덱스를 추출
                int itemIndex = (int)gameData.equipItem[i].itemType;

                //아이템의 부모를 Slot 게임 오브젝트로 변경
                itemObjects[itemIndex].GetComponent<Transform>().SetParent(slots[j]);
                //아이템의 ItemInfo 클래스의 itemData에 로드한 데이터 값을 저장
                itemObjects[itemIndex].GetComponent<ItemInfo>().itemData = gameData.equipItem[i];

                //아이템을 Slot에 추가하면 바깥 fot 구문으로 빠져나감.
                break;
            }
        }
    }

    void SaveGameData()
    {
        //dataManager.Save(gameData);
        //.asset 파일에 데이터 저장
        UnityEditor.EditorUtility.SetDirty(gameData);
    }

    public void IncKillCount()
    {
        ++gameData.killCount;
        killCountTxt.text = "KILL " + gameData.killCount.ToString("0000");
        //PlayerPrefs.SetInt("KILL_COUNT", killCount);
    }

    IEnumerator CreateEnemy()
    {
        while(!isGameOver)
        {
            int enemyCount = (int)GameObject.FindGameObjectsWithTag("ENEMY").Length;

            if (enemyCount < maxEnemy)
            {
                yield return new WaitForSeconds(createTime);

                int idx = Random.Range(1, points.Length);
                Instantiate(enemy, points[idx].position, points[idx].rotation);
            }
            else yield return null;
        }
    }

    public GameObject GetBullet()
    {
        for(int i=0; i<bulletPool.Count; i++)
        {
            if(bulletPool[i].activeSelf == false)
            {
                return bulletPool[i];
            }
        }

        return null;
    }

    public void CreatePooling()
    {
        GameObject objectPools = new GameObject("ObjectPools");

        for(int i = 0; i<maxPool; i++)
        {
            var obj = Instantiate(bulletPrefab, objectPools.transform);
            obj.name = "Bullet_" + i.ToString("00");
            obj.SetActive(false);
            bulletPool.Add(obj);
        }

    }

    public void OnPauseClick()
    {
        //일시 정지 값을 토글시킴
        isPaused = !isPaused;
        Time.timeScale = (isPaused) ? 0.0f : 1.0f;

        var playerObj = GameObject.FindGameObjectWithTag("PLAYER");
        var scripts = playerObj.GetComponents<MonoBehaviour>();
        foreach(var script in scripts)
        {
            script.enabled = !isPaused;
        }

        var canvasGroup = GameObject.Find("Panel - Weapon").GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = !isPaused;
    }

    public void OnInventoryOpen(bool isOpened)
    {
        inventoryCG.alpha = (isOpened) ? 1.0f : 0.0f;
        inventoryCG.interactable = isOpened;
        inventoryCG.blocksRaycasts = isOpened;
    }
}
