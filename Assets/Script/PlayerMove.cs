using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;  // 게임 관리 스크립트 참조
    public float maxSpeed;          // 플레이어의 최대 속도
    public float jumpPower;         // 점프 시 가해지는 힘의 크기
    Rigidbody2D rigid;              // Rigidbody2D 컴포넌트 참조
    SpriteRenderer spriteRenderer;  // SpriteRenderer 컴포넌트 참조
    Animator anim;                  // Animator 컴포넌트 참조

    // 초기화
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 점프 처리
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);  // 위쪽으로 힘을 가함
            anim.SetBool("isJumping", true);  // 점프 상태 설정
        }

        // 수평 이동 속도 조절 (키를 뗐을 때 감속)
        if (Input.GetButtonUp("Horizontal"))
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);

        // 방향에 따른 스프라이트 반전 처리
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // 걷기 애니메이션 상태 전환
        if (Mathf.Abs(rigid.velocity.x) < 0.4f)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        // 수평 입력에 따른 힘 가하기
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // 최대 속도 제한
        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < -maxSpeed)
            rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);

        // 낙하 중 점프 상태 해제
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector2.down, new Color(0, 1, 0)); // 디버그용 레이 표시
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)  // 플랫폼과 일정 거리 이내이면 점프 상태 해제
                    anim.SetBool("isJumping", false);
            }
        }
    }

    // 적과의 충돌 처리
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // 적을 밟았을 때 공격 처리
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            // 그렇지 않으면 피해 처리
            else
                OnDamaged(collision.transform.position);
        }
    }

    // 아이템 또는 스테이지 끝과의 충돌 처리
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // 아이템 종류에 따른 점수 추가
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;

            // 아이템 비활성화
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            gameManager.NextStage();  // 스테이지 전환 호출
        }
    }

    // 적 공격 처리
    void OnAttack(Transform enemy)
    {
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);  // 위쪽으로 반발력 적용
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();  // 적의 피해 처리 메서드 호출
    }

    // 플레이어 피해 처리
    void OnDamaged(Vector2 targetPos)
    {
        gameManager.health--;  // 체력 감소

        // 무적 상태 설정
        gameObject.layer = 11;  // 무적 레이어로 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);  // 반투명 색상 적용

        // 반발력 적용
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1; // 피해 방향 계산
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        anim.SetTrigger("doDamaged");  // 피해 애니메이션 트리거
        Invoke("OffDamaged", 1.5f);  // 일정 시간 후 무적 상태 해제
    }

    // 무적 상태 해제
    void OffDamaged()
    {
        gameObject.layer = 10;  // 원래 레이어로 복원
        spriteRenderer.color = new Color(1, 1, 1, 1);  // 원래 색상 복원
    }
}
