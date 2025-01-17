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
	/// This logic provides the necessary behaviour to manage the player's board during a gameplay session
	/// </summary>
	public interface IDeckSpawnerDataProvider
	{
		IObservableListReader<UniqueId> Deck { get; }
	}

	/// <inheritdoc />
	public interface IDeckSpawnerLogic : IDeckSpawnerDataProvider
	{
		new IObservableList<UniqueId> Deck { get; }

		void RefillDeck();
	}

	/// <inheritdoc cref="ITileBoardLogic"/>
	public class DeckSpawnerLogic : AbstractBaseLogic<PlayerData>, IDeckSpawnerLogic, IGameLogicInitializer
	{
		private IObservableList<UniqueId> _deck;

		/// <inheritdoc />
		public IObservableList<UniqueId> Deck => _deck;
		/// <inheritdoc />
		IObservableListReader<UniqueId> IDeckSpawnerDataProvider.Deck => _deck;

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
			Deck.Clear();

			for (var i = 0; i < Constants.Gameplay.Max_Deck_Pieces; i++)
			{
				Deck.Add(GameLogic.EntityFactoryLogic.CreatePiece().Id);
			}
		}
	}
}
