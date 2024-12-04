using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer SpriteRenderer;
    public GameObject prfHpBar;
    public GameObject canvas;
    RectTransform hpBar;
    public string enemyName;
    public int maxHp;
    public int nowHp;
    public int atkDmg;
    public int atkSpeed;
    public float moveSpeed = 3f; // 기본 이동 속도
    Image nowHpbar;
    public float height = 1.7f;
    public float detectionRange = 5f;
    public int nextMove;

    Transform player;
    bool isChasing = false;

    private Vector3 initialPosition;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
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
        initialPosition = transform.position;
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
        if (isChangingDirection == false)
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
            isChasing = true; // 플레이어가 범위 안에 있으면 추적 시작
        }
        else
        {
            isChasing = false; // 범위 밖이면 추적 멈춤
        }

        // 플레이어를 추적
        if (isChasing)
        {
            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 3, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null && !isChangingDirection) // 벽 끝에 닿은 경우 (반복하지 않도록)
            {
                MoveAwayAndRetry(); // 벽 끝에서 처리하는 로직 호출
                isChangingDirection = true; // 반대 방향으로 이동 중임을 표시
            }
            else
            {
                // 추적 로직
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                LookAtPlayer();
            }
        }
    }

    private bool isChangingDirection = false; // 벽 끝에 닿을 때 이동 상태 관리

    private void MoveAwayAndRetry()
    {
        Debug.Log("Hit wall, stopping movement for a moment.");

        // 이동 속도를 0으로 설정하여 멈춤
        moveSpeed = 0f;

        // 잠시 멈춘 후 추적을 재개
        StartCoroutine(ResumeMovementAfterDelay()); // 일정 시간 후 이동을 재개하는 코루틴 호출
    }

    private IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForSeconds(2f); // 1초 동안 대기
        
        moveSpeed = 3f; // 기본 이동 속도 (원하는 값으로 설정)
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime * 5f);
        isChangingDirection = false; // 다시 추적을 시작할 수 있도록 상태 변경
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            // 플레이어가 오른쪽에 있으면
            transform.localScale = new Vector3(1, 1, 1); // 오른쪽으로 반전
        }
        else
        {
            // 플레이어가 왼쪽에 있으면
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽으로 반전
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}