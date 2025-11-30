using System;
using PawBab.DesignPatterns.Observer;
using TMPro;
using UnityEngine;

namespace PawBab.DesignPatterns.Examples.Observer
{
    public class ScoreView : EventListenerBehaviour<ScoreChangedEvent>
    {
        [SerializeField]
        private TMP_Text scoreText;

        public override void OnEvent(ScoreChangedEvent evt)
        {
            if (scoreText == null)
                throw new NullReferenceException("scoreText is null");

            scoreText.text = $"Score: {evt.NewScore}";
        }
    }
}