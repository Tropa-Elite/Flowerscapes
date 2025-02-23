using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;

namespace Game.Logic.Client
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the player's piece deck board during a gameplay session
	/// </summary>
	public interface IDeckSpawnerDataProvider
	{
		IObservableListReader<UniqueId> Deck { get; }
	}

	/// <inheritdoc />
	public interface IDeckSpawnerLogic : IDeckSpawnerDataProvider
	{
		void RefillDeck();
		void Remove(UniqueId pieceId);
	}

	/// <inheritdoc cref="ITileBoardLogic"/>
	public class DeckSpawnerLogic : AbstractBaseLogic<PlayerData>, IDeckSpawnerLogic, IGameLogicInitializer
	{
		private IObservableList<UniqueId> _deck;

		/// <inheritdoc />
		public IObservableListReader<UniqueId> Deck => _deck;

		public DeckSpawnerLogic(
			IGameLogicLocator gameLogic, 
			IConfigsProvider configsProvider, 
			IDataProvider dataProvider, 
			ITimeService timeService) :
			base(gameLogic, configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_deck = new ObservableList<UniqueId>(Data.Deck);
		}

		/// <inheritdoc />
		public void RefillDeck()
		{
			_deck.Clear();

			for (var i = 0; i < _deck.Count; i++)
			{
				_deck.Add(GameLogic.EntityFactoryLogic.CreatePiece().Id);
			}
		}

		/// <inheritdoc />
		public void Remove(UniqueId pieceId)
		{
			var index = _deck.IndexOf(pieceId);
			
			if (index < 0)
			{
				throw new LogicException($"There isn't any piece with id {pieceId} in the deck");
			}
			
			_deck[index] = UniqueId.Invalid;
		}
	}
}
