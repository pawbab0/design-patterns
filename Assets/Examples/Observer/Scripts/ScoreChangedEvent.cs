using PawBab.DesignPatterns.Observer;

namespace PawBab.DesignPatterns.Examples.Observer
{
    public readonly struct ScoreChangedEvent : IEvent
    {
        public int NewScore { get; }

        public ScoreChangedEvent(int newScore)
        {
            NewScore = newScore;
        }
    }
}