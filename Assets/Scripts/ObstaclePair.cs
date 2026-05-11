using UnityEngine;

public class ObstaclePair : MonoBehaviour
{
    [Header("UI")]
    public GameObject scorePopupPrefab;

    private float speed;
    private bool scoreCounted = false;
    private GameObject player;
    private Transform popupParent;

    void Start()
    {
        popupParent = GameObject.Find("PopupsContainer")?.transform;
    }

    public void Init(float moveSpeed)
    {
        speed = moveSpeed;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < -12)
        {
            Destroy(gameObject);
        }

        if (!scoreCounted && player != null)
        {
            float obstacleRightEdge = transform.position.x + 2.5f;
            float playerLeftEdge = player.transform.position.x - 0.5f;

            if (obstacleRightEdge < playerLeftEdge)
            {
                scoreCounted = true;
                GameManager.Instance.AddScore();

                if (scorePopupPrefab != null)
                {
                    SpawnScorePopup();
                }
            }
        }
    }

    private void SpawnScorePopup()
    {
        if (popupParent != null)
        {
            Instantiate(scorePopupPrefab, popupParent);
        }
    }
}