using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour {

    public GameObject expEffect;

    public Mesh[] meshes;
    public Texture[] texture;

    public float expRadius = 10.0f;
    public AudioClip expSfx;

    private int hitCount;
    private Rigidbody rb;

    private MeshFilter meshFilter;
    private MeshRenderer _renderer;

    private AudioSource _audio;
    private Shake shake;
    
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        meshFilter = GetComponent<MeshFilter>();

        _renderer = GetComponent<MeshRenderer>();
        _renderer.material.mainTexture = texture[Random.Range(0, texture.Length)];

        _audio = GetComponent<AudioSource>();
        //shake = GameObject.Find("CameraRig").GetComponent<Shake>();
        StartCoroutine(GetShake());
    }

    IEnumerator GetShake()
    {
        while(!UnityEngine.SceneManagement.SceneManager.GetSceneByName("Play").isLoaded)
        {
            yield return null;
        }
        shake = GameObject.Find("CameraRig").GetComponent<Shake>();
    }

    private void OnCollisionEnter(Collision coll)
    {

        if (coll.collider.CompareTag("BULLET"))
        {
            if (++hitCount == 3)
            {
                ExpBarrel();
            }
        }
    }

    private void ExpBarrel()
    {
        GameObject effect = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2.0f);
        rb.mass = 1.0f;
        rb.AddForce(Vector3.up * 1000.0f);

        IndirectDamage(transform.position);

        //난수를 발생시켜서 랜덤한 (찌그러진)메쉬를 적용시킴
        int idx = Random.Range(0, meshes.Length);
        meshFilter.sharedMesh = meshes[idx];
        GetComponent<MeshCollider>().sharedMesh = meshes[idx];

        rb.mass = 1f;
        hitCount = 0;

        _audio.PlayOneShot(expSfx, 1.0f);
        StartCoroutine(shake.ShakeCamera(0.1f, 0.2f, 0.5f));
    }

    private void IndirectDamage(Vector3 pos)
    {
        Collider[] colls = Physics.OverlapSphere(pos, expRadius, 1 << 9);

        foreach (var coll in colls)
        {
            var _rb = coll.GetComponent<Rigidbody>();

            _rb.mass = 1.0f;
            _rb.AddExplosionForce(1200.0f, pos, expRadius, 1000.0f);

        }
    }
}
