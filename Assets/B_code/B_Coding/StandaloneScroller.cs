using UnityEngine;

public class StandaloneScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float segmentWidth = 20f;
    [SerializeField] private int totalSegments = 2;

    private void Update()
    {
        transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);

        if (transform.position.x <= -segmentWidth)
        {
            transform.position += new Vector3(segmentWidth * totalSegments, 0f, 0f);
        }
    }
}