﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGameEngine.EventSystem;
using CardGameEngine.EventSystem.Events.CardEvents;
using MoonSharp.Interpreter.Interop;

namespace CardGameEngine.Cards.CardPiles
{
    /// <summary>
    /// Classe représentant une pile de cartes comme la main, le deck ou la défausse
    /// </summary>
    public class CardPile : IEnumerable<Card>
    {
        private readonly Game _game;

        /// <summary>
        /// Liste des cartes dans la pile
        /// </summary>
        private List<Card> _cardList;

        /// <summary>
        /// EventManager de la partie
        /// </summary>
        [MoonSharpVisible(false)]
        protected EventManager EventManager { get; }

        /// <summary>
        /// Nombre de cartes maximal dans la pile
        /// </summary>
        public int? LimitSize { get; }

        /// <summary>
        /// Propriété renvoyant le nombre de cartes
        /// </summary>
        public int Count => _cardList.Count;

        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Accesseur de la liste
        /// </summary>
        /// <param name="i">Index de la carte</param>
        public Card this[int i] => _cardList.ElementAt(i);


        /// <summary>
        /// Constructeur de la pile vide
        /// </summary>
        /// <param name="eventManager">EventManager de la partie</param>
        /// <param name="limitSize">Taille limite de la pile, null pour aucune limite</param>
        internal CardPile(Game game, int? limitSize = null)
        {
            this._game = game;
            EventManager = game.EventManager;
            _cardList = new List<Card>();
            LimitSize = limitSize;
        }

        /// <summary>
        /// Constructeur de la pile remplie
        /// </summary>
        /// <param name="eventManager">EventManager de la partie</param>
        /// <param name="cards"></param>
        internal CardPile(Game game, List<Card> cards)
        {
            EventManager = game.EventManager;
            _game = game;
            _cardList = cards;
            LimitSize = null;
        }


        /// <summary>
        /// Méthode indiquant si la pile contient une certaine carte
        /// </summary>
        /// <param name="card">La carte à chercher</param>
        /// <returns>Un booléen en fonction de la présence de la carte</returns>
        public bool Contains(Card card)
        {
            return _cardList.Contains(card);
        }

        /// <summary>
        /// Méthode renvoyant la position d'une carte dans la pile
        /// </summary>
        /// <param name="card">La carte à chercher</param>
        /// <returns>Un entier selon la position</returns>
        public int IndexOf(Card card)
        {
            return _cardList.IndexOf(card);
        }

        /// <summary>
        /// Insère une carte dans la liste en décalant les autres
        /// </summary>
        /// <param name="newPosition">Nouvelle position</param>
        /// <param name="card">La carte à insérer</param>
        private void Insert(int newPosition, Card card)
        {
            if (card.IsVirtual)
                throw new InvalidOperationException(
                    $"Tentative d'insertion de la carte virtuelle {card} dans une pile");
            _cardList.Insert(newPosition, card);
        }

        /// <summary>
        /// Renvoie un itérateur pour la liste
        /// </summary>
        /// <returns>L'itérateur créé</returns>
        [MoonSharpVisible(false)]
        IEnumerator<Card> IEnumerable<Card>.GetEnumerator()
        {
            return _cardList.GetEnumerator();
        }

        /// <summary>
        /// Renvoie un itérateur pour la liste
        /// </summary>
        /// <returns>L'itérateur créé</returns>
        public IEnumerator GetEnumerator()
        {
            return _cardList.GetEnumerator();
        }

        /// <summary>
        /// Déplace une carte dans une autre pile à une position donnée
        /// </summary>
        /// <param name="newCardPile">La nouvelle ciblée</param>
        /// <param name="card">La carte à bouger</param>
        /// <param name="newPosition">La position à prendre</param>
        /// <exception cref="NotInPileException">Si la carte n'est pas dans la pile</exception>
        [MoonSharpVisible(true)]
        internal virtual bool MoveTo(CardPile newCardPile, Card card, int newPosition)
        {
            if (!_cardList.Contains(card))
            {
                //throw new NotInPileException(card);
                if (!card.IsVirtual)
                    _game.Log(GetType().Name,
                        $"Tentative de déplacement de {card} depuis {this} vers {newCardPile}, alors qu'elle n'est pas dans {this}");
                return false;
            }

            if (newCardPile.LimitSize != null && newCardPile.Count + 1 > newCardPile.LimitSize)
            {
                return false;
            }

            CardMovePileEvent moveEvent = new CardMovePileEvent(card, this, IndexOf(card), newCardPile, newPosition);
            using (var postEvent = EventManager.SendEvent(moveEvent))
            {
                if (postEvent.Event.Cancelled)
                {
                    return false;
                }

                if (!_cardList.Remove(postEvent.Event.Card))
                {
                    throw new InvalidOperationException(
                        $"La carte {card} a disparue de la pile {this} pendant l'opération de déplacement vers {postEvent.Event.DestPile}");
                }

                postEvent.Event.DestPile.Insert(postEvent.Event.DestIndex, postEvent.Event.Card);
            }

            return true;
        }

        /// <summary>
        /// Déplace une carte dans sa pile à une position donnée
        /// </summary>
        /// <param name="card">La carte à bouger</param>
        /// <param name="newPosition">La position à prendre</param>
        /// <exception cref="NotInPileException">Si la carte n'est pas dans la pile</exception>
        [MoonSharpVisible(true)]
        internal bool MoveInternal(Card card, int newPosition)
        {
            return MoveTo(this, card, newPosition);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}[{string.Join(", ", _cardList)}]";
        }
    }
}