using System;
using UnityEngine;
using UnityEngine.Internal.Experimental.UIElements;
using UnityEngine.UI;

namespace PolePosition.UI
{
    public class PlayerResultsPanelController : MonoBehaviour
    {
        [SerializeField] private Text _positionText;
        [SerializeField] private Image _color;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _totalTimeText;
        [SerializeField] private Text _lapTimeText;
        
        public void UpdateName(string playerName)
        {
            _nameText.text = playerName;
        }

        public void UpdatePosition(int position)
        {
            _positionText.text = "" + position;
        }

        public void UpdateTotaltime(float totalTime)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(totalTime * 1000);
            _totalTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D4}", timeSpan.Hours, 
                timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        public void UpdateLaptime(float lapTime)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(lapTime * 1000);
            _lapTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D4}", timeSpan.Hours, 
                timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        public void UpdateColor(Color color)
        {
            _color.color = color;
        }
        
        public void UpdateData(int position, Color color, string playerName, float totalTime, float lapTime)
        {
            UpdatePosition(position);
            UpdateColor(color);
            UpdateName(playerName);
            UpdateTotaltime(totalTime);
            UpdateLaptime(lapTime);
        }
    }
}