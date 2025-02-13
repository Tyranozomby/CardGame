﻿using CardGameEngine.Cards;
using CardGameEngine.Cards.CardPiles;

namespace CardGameEngine.EventSystem.Events.CardEvents
{
    /// <summary>
    /// Évènement annulable correspondant au changement de pile d'une carte
    /// </summary>
    public class CardMovePileEvent : CardEvent
    {
        /// <summary>
        /// Pile d'origine
        /// </summary>
        public CardPile SourcePile { get; }

        /// <summary>
        /// Position dans la pile d'origine
        /// </summary>
        public int SourceIndex { get; }

        /// <summary>
        /// Pile de destination
        /// </summary>
        public CardPile DestPile { get; internal set; }

        /// <summary>
        /// Position dans la pile de destination
        /// </summary>
        public int DestIndex { get; internal set; }

        internal CardMovePileEvent(Card card, CardPile sourcePile, int sourceIndex, CardPile destPile, int destIndex) :
            base(card)
        {
            SourcePile = sourcePile;
            SourceIndex = sourceIndex;
            DestPile = destPile;
            DestIndex = destIndex;
        }
    }
}