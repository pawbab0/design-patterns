using PawBab.DesignPatterns.Observer;
using UnityEngine;

namespace PawBab.DesignPatterns.Examples.Observer
{
    public class ScoreManager : MonoBehaviour
    {
        private int _score;

        public int Score => _score;

        public void AddPoints(int amount)
        {
            _score += amount;
            GlobalEventBus.Instance.Publish(new ScoreChangedEvent(_score));
        }

        public void AddPoints(string fieldValue)
        {
            if(int.TryParse(fieldValue, out var result))
                AddPoints(result);
        }
    }
}