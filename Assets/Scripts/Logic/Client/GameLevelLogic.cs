using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Logic.Client
{
	/// <summary>
	///  This logic provides the necessary behaviour to manage the player's game level state
	/// </summary>
	public interface IGameLevelDataProvider
	{
		/// <summary>
		/// Provides read-only access to the current level XP
		/// </summary>
		IObservableFieldReader<int> LevelXp { get; }

		/// <summary>
		/// Checks if the game is over
		/// </summary>
		bool IsGameOver();

		/// <summary>
		/// Checks if the current level is complete
		/// </summary>
		bool IsLevelCompleted();
	}

	/// <inheritdoc />
	public interface IGameLevelLogic : IGameLevelDataProvider
	{
		/// <summary>
		/// The current XP of the level
		/// </summary>
		new IObservableField<int> LevelXp { get; }

		/// <summary>
		/// Adds XP to the current level
		/// </summary>
		/// <param name="xpToAdd">Amount of XP to add to the current level</param>
		void AddLevelXp(int xpToAdd);
	}

	/// <inheritdoc cref="IGameLevelLogic"/>
	public class GameLevelLogic : AbstractBaseLogic<PlayerData>, IGameLevelLogic, IGameLogicInitializer
	{
		private IObservableField<int> _levelXp;

		/// <inheritdoc />
		IObservableFieldReader<int> IGameLevelDataProvider.LevelXp => _levelXp;

		/// <inheritdoc />
		public IObservableField<int> LevelXp => _levelXp;

		public GameLevelLogic(
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
			_levelXp = new ObservableResolverField<int>(() => Data.CurrentLevelXp, xp => Data.CurrentLevelXp = xp);
		}

		/// <inheritdoc />
		public bool IsGameOver()
		{
			var tileCount = Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns;
			var pieceCount = GameLogic.PieceDataProvider.Pieces.Count;
			var pieceDeckCount = GameLogic.DeckSpawnerDataProvider.Deck.Count;

			if(pieceCount - pieceDeckCount < tileCount) return false;

			for (var i = 0; i < Constants.Gameplay.Board_Rows; i++)
			{
				for (var j = 0; j < Constants.Gameplay.Board_Columns; j++)
				{
					if (Data.Board[i, j] == null || !Data.Board[i, j].PieceId.IsValid)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc />
		public bool IsLevelCompleted()
		{
			return _levelXp.Value >= Constants.Gameplay.Level_Max_Xp;
		}

		/// <inheritdoc />
		public void AddLevelXp(int xpToAdd)
		{
			var levelMaxXp = Constants.Gameplay.Level_Max_Xp;
			var currentXp = _levelXp.Value + xpToAdd;

			_levelXp.Value = Math.Min(currentXp, levelMaxXp);
		}
	}
}
