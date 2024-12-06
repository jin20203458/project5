using UnityEngine;

public class DynamicBackground : MonoBehaviour
{
    public GameObject backgroundPrefab; // 배경 프리팹
    public float backgroundHeight; // 배경 높이
    public int poolSize = 5; // 배경 풀의 크기
    public float spawnDistance = 10f; // 배경 생성 거리

    private Transform player; // 플레이어 트랜스폼
    private GameObject[] backgroundPool; // 배경 풀

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 배경 풀 초기화
        backgroundPool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            // 배경 프리팹을 인스턴스화하여 풀에 넣음
            backgroundPool[i] = Instantiate(backgroundPrefab, new Vector3(0, i * backgroundHeight, 0), Quaternion.identity);
            backgroundPool[i].SetActive(false); // 처음에는 비활성화
        }

        // 첫 번째 배경을 활성화하고 위치 설정
        ActivateBackground(0);
    }

    void Update()
    {
        // 플레이어가 이동할 때마다 배경을 체크하고 생성/삭제
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bg = backgroundPool[i];
            if (!bg.activeSelf) continue; // 비활성화된 배경은 무시

            // 배경이 화면 밖으로 나가면 비활성화
            if (bg.transform.position.y + backgroundHeight < player.position.y - spawnDistance)
            {
                bg.SetActive(false);
            }
        }

        // 플레이어가 일정 거리 이동했을 때 배경을 활성화
        for (int i = 0; i < poolSize; i++)
        {
            if (!backgroundPool[i].activeSelf)
            {
                ActivateBackground(i);
                break;
            }
        }
    }

    // 배경을 활성화하고 적절한 위치에 배치
    private void ActivateBackground(int index)
    {
        backgroundPool[index].SetActive(true);
        float yPosition = player.position.y + spawnDistance + index * backgroundHeight;
        backgroundPool[index].transform.position = new Vector3(0, yPosition, 0);
    }
}