using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public GameObject prfHpBar;
    public GameObject canvas;
    RectTransform hpBar;
    public string enemyName;
    public int maxHp;
    public int nowHp;
    public int atkDmg;
    public int atkSpeed;
    public float moveSpeed = 3f;
    Image nowHpbar;
    public float height = 1.7f;
    public float detectionRange = 5f;

    Transform player;
    bool isChasing = false;

    void Start()
    {
        // 체력 바 초기화 (각 적에 대해 독립적으로 생성)
        hpBar = Instantiate(prfHpBar, canvas.transform).GetComponent<RectTransform>();

        // 적 상태 설정
        if (name.Equals("Enemy1"))
        {
            SetEnemyStatus("Enemy1", 100, 10, 1); // Enemy1의 상태 설정
        }
        else if (name.Equals("Enemy2"))
        {
            SetEnemyStatus("Enemy2", 80, 12, 2); // Enemy2의 상태 설정 (예시)
        }
        else
        {
            // 여기에 다른 적 상태 초기화 코드 추가
            SetEnemyStatus("Enemy3", 150, 8, 1); // 추가적인 적 초기화
        }

        // 체력 바의 이미지 초기화
        nowHpbar = hpBar.transform.GetChild(0).GetComponent<Image>();

        // 초기화된 값 확인 (디버깅용)
        Debug.Log($"Enemy Name: {enemyName}, Max HP: {maxHp}, Current HP: {nowHp}");

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player object not found!");
        }
    }

    void Update()
    {
        // 체력 바 위치 갱신
        Vector3 _hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + height, 0));
        hpBar.position = _hpBarPos;

        // 체력 바 상태 갱신
        if (nowHpbar != null) // nowHpbar가 null이 아닌 경우에만 갱신
        {
            nowHpbar.fillAmount = (float)nowHp / (float)maxHp;
        }

        // 플레이어 탐지 및 추적
        DetectAndChasePlayer();
    }

    public void TakeDamage(int damage)
    {
        nowHp -= damage;
        Debug.Log("Damage taken: " + damage + ", Remaining HP: " + nowHp);

        if (nowHp <= 0)
        {
            Destroy(gameObject);
            Destroy(hpBar.gameObject);
        }
    }

    private void SetEnemyStatus(string _enemyName, int _maxHp, int _atkDmg, int _atkSpeed)
    {
        enemyName = _enemyName;
        maxHp = _maxHp;
        nowHp = _maxHp;
        atkDmg = _atkDmg;
        atkSpeed = _atkSpeed;
    }

    private void DetectAndChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어가 탐지 범위 안에 있을 경우에만 추적
        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        // 플레이어를 추적
        if (isChasing)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            LookAtPlayer();
        }
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
