using TMPro;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private TextMeshProUGUI dateText = null;
    [SerializeField] private TextMeshProUGUI seasonText = null;
    [SerializeField] private TextMeshProUGUI yearText = null;

    private void OnEnable()
    {
        EventHandler.GameTimeChangedEvent += OnGameTimeChangedEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameTimeChangedEvent -= OnGameTimeChangedEvent;
    }

    private void OnGameTimeChangedEvent(GameDateTime gameDateTime, TimeChangeFlags timeChangeFlags)
    {
        int displayHour = gameDateTime.Hour % 12;
        if (displayHour == 0)
        {
            displayHour = 12;
        }

        string amPm = gameDateTime.Hour < 12 ? "am" : "pm";
        timeText.text = displayHour.ToString("D2") + ":" + gameDateTime.Minute.ToString("D2") + " " + amPm;
        dateText.text = gameDateTime.DayOfWeek + ". " + gameDateTime.Day.ToString();
        seasonText.text = gameDateTime.Season.ToString();
        yearText.text = "Year " + gameDateTime.Year.ToString();
    }
}
