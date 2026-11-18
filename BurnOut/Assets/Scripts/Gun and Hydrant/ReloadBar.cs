using UnityEngine;

public class ReloadBar : MonoBehaviour
{
    private bool reloading;
    float x;
    float y;
    float z;
    [SerializeField] private GameObject bar;

    void Start()
    {
        x = 2;
        y = bar.transform.localScale.y;
        z = bar.transform.localScale.z;
        reloading = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (reloading)
        {
            x -= Time.deltaTime;
            bar.transform.localScale = new Vector3(x, y, z);
            if(x <= 0)
            {
                reloading = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void reload()
    {
        x = 2;
        gameObject.SetActive(true);
        reloading = true;
    }


}
