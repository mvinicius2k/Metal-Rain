using UnityEngine;
using UnityEngine.Pool;

public class Weapon_MB : MonoBehaviour
{
    public TankAttack_MB Attack;
    public GameObject BulletPrefab => Data.BulletPrefab;
    public Transform FirePoint;





    [Header("Criação do Pool")] //Executados somente uma vez durante a primeira instanciação da instância de pool
    public bool CheckPool = true;
    public int DefaultPoolSize = 30;
    public int MaxPoolSize = 100;



    private ObjectPool<GameObject> bulletPool;
    public ObjectPool<GameObject> BulletPool => bulletPool;
    private float fireDelayCount;
    public TankStatsBase Data => Attack.Tank.Stats;

    #region PoolMethods

    private GameObject CreateBullet()
    {
        var bulletObj = Instantiate(BulletPrefab, Vector3.zero, transform.rotation);
        var bullet = bulletObj.GetComponent<Bullet_MB>();
        bullet.Initialize(this);
        bulletObj.SetActive(false);
        return bulletObj;
    }

    private void OnTakeFromPool(GameObject bulletObj) //Obtendo do pool e preparando
    {
        bulletObj.SetActive(true);
        bulletObj.transform.position = FirePoint.position;
        bulletObj.transform.rotation = FirePoint.rotation;

    }

    private void OnReturnedToPool(GameObject bulletObj) // Desativando bala e limpando-a
        => bulletObj.SetActive(false);
    private void OnDestroyBulletObject(GameObject bulletObj)  // Destruindo bala quando o pool já está cheio ou quando o objeto for destruido
        => Destroy(bulletObj);

    #endregion

    public bool TryFire()
    {

        if (fireDelayCount > 0f)
            return false;
        //Debug.Log("Atirando");
        fireDelayCount = Data.ShootDelay;
        var bulletObj = bulletPool.Get();
        return true;



    }

    private void Awake()
    {
        bulletPool = new ObjectPool<GameObject>(
                        CreateBullet, OnTakeFromPool, OnReturnedToPool, OnDestroyBulletObject,
                        CheckPool, DefaultPoolSize, MaxPoolSize);
    }

    private void Update()
    {
        if (fireDelayCount > 0f && SpawnFieldManager_MB.Instance.Started)
            fireDelayCount -= Time.deltaTime;


    }
}
