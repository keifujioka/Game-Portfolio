using UnityEngine;
using UnityEngine.UI; 

public class  Progressive : MonoBehaviour
{
    public Image cooldown;
    public bool coolingDown;
    public float waitTime = 99f;

    // Update is called once per frame
    void Update()
    {
        if (coolingDown == true)
        {
            cooldown.fillAmount -= 1.0f / waitTime * Time.deltaTime;
        }
    }
}