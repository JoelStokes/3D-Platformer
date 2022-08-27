using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DistanceDisplay : MonoBehaviour
{
    public bool distance = false;   //Switch between Distance & Room mode
    public bool vertical = false;   //Switch between Vertical & Horizontal for Distance

    public TextMeshProUGUI playerInfo;
    public TextMeshProUGUI wheelInfo;

    public GameObject Player;
    public GameObject Wheel;

    private float distanceAdjust;
    private int roomCount;

    private float wheelTimer;
    private float timerAdd = 10;    //Seconds to add when room beaten
    private float wheelAddition = 1.5f;    //Ammount to add to Wheel Distance Counter to better match Player Distance

    // Start is called before the first frame update
    void Start()
    {
        distanceAdjust = Player.transform.position.y;   //Subtract amount is where player starts the beginning of the level
    }

    // Update is called once per frame
    void Update()
    {
        playerInfo.SetText(Mathf.Round(Player.transform.position.y - distanceAdjust).ToString("0"));
        wheelInfo.SetText(Mathf.Round((Wheel.transform.position.y - distanceAdjust) + wheelAddition).ToString("0"));
    }
}
