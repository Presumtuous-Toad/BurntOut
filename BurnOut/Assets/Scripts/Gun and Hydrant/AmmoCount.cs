using TMPro;
using UnityEngine;

public class AmmoCount : MonoBehaviour
{
    [SerializeField] private GunController gunController;
    [SerializeField]private TextMeshProUGUI textMeshPro;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = "Ammo: " + gunController.ammo.ToString();
    }
}
