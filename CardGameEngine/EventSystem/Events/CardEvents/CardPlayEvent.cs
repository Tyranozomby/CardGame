﻿using CardGameEngine.Cards;
using CardGameEngine.GameSystems;

namespace CardGameEngine.EventSystem.Events.CardEvents
{
    /// <summary>
    /// Évènement annulable correspondant à l'activation d'une carte entiere (du cout en PA jusqu'a la défausse)
    /// </summary>
    public class CardPlayEvent : TransferrableCardEvent
    {
        public Player WhoPlayed { get; internal set; }

        internal CardPlayEvent(Player whoPlayed, Card card) : base(card)
        {
            WhoPlayed = whoPlayed;
        }
    }
}