using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Logic.Shared;
using Game.Data;
using Newtonsoft.Json;
using System;
using Game.Services;

namespace Game.Logic
{
	/// <summary>
	/// This interface marks the Game Logic as one that needs to initialize it's internal state
	/// </summary>
	public interface IGameLogicInitializer
	{
		/// <summary>
		/// Initializes the Game Logic state to it's default initial values
		/// </summary>
		void Init();
	}
	
	/// <summary>
	/// Provides access to all game's data.
	/// This interface provides the data with view only permissions
	/// </summary>
	public interface IGameDataProvider
	{
		/// <inheritdoc cref="IAppDataProvider"/>
		IAppDataProvider AppDataProvider { get; }
		/// <inheritdoc cref="IRngDataProvider"/>
		IRngDataProvider RngDataProvider { get; }
		/// <inheritdoc cref="IEntityFactoryDataProvider"/>
		IEntityFactoryDataProvider EntityFactoryDataProvider { get; }
		/// <inheritdoc cref="ICurrencyDataProvider"/>
		ICurrencyDataProvider CurrencyDataProvider { get; }
		/// <inheritdoc cref="IGameplayBoardDataProvider"/>
		IGameplayBoardDataProvider GameplayBoardDataProvider { get; }
	}

	/// <summary>
	/// Provides access to all game's logic
	/// This interface shouldn't be exposed to the views or controllers
	/// To interact with the logic, execute a <see cref="Commands.IGameCommand"/> via the <see cref="ICommandService"/>
	/// </summary>
	public interface IGameLogic : IGameDataProvider
	{
		/// <inheritdoc cref="IAppLogic"/>
		IAppLogic AppLogic { get; }
		/// <inheritdoc cref="IRngLogic"/>
		IRngLogic RngLogic { get; }
		/// <inheritdoc cref="IEntityFactoryLogic"/>
		IEntityFactoryLogic EntityFactoryLogic { get; }
		/// <inheritdoc cref="ICurrencyLogic"/>
		ICurrencyLogic CurrencyLogic { get; }
		/// <inheritdoc cref="IGameplayBoardLogic"/>
		IGameplayBoardLogic GameplayBoardLogic { get; }
	}

	/// <summary>
	/// This interface provides the contract to initialize the Game Logic
	/// </summary>
	public interface IGameLogicInit
	{
		/// <summary>
		/// Initializes the Game Logic state to it's default initial values
		/// </summary>
		void Init(IDataService dataService, IGameServices gameServices);
	}

	/// <inheritdoc cref="IGameLogic"/>
	public class GameLogic : IGameLogic, IGameLogicInit
	{
		/// <inheritdoc />
		public IAppDataProvider AppDataProvider => AppLogic;
		/// <inheritdoc />
		public IRngDataProvider RngDataProvider => RngLogic;
		/// <inheritdoc />
		public IEntityFactoryDataProvider EntityFactoryDataProvider => EntityFactoryLogic;
		/// <inheritdoc />
		public ICurrencyDataProvider CurrencyDataProvider => CurrencyLogic;
		/// <inheritdoc />
		public IGameplayBoardDataProvider GameplayBoardDataProvider => GameplayBoardLogic;

		/// <inheritdoc />
		public IAppLogic AppLogic { get; }
		/// <inheritdoc />
		public IRngLogic RngLogic { get; private set; }
		/// <inheritdoc />
		public IEntityFactoryLogic EntityFactoryLogic { get; }
		/// <inheritdoc />
		public ICurrencyLogic CurrencyLogic { get; }
		/// <inheritdoc />
		public IGameplayBoardLogic GameplayBoardLogic { get; }

		public GameLogic(IInstaller installer)
		{
			var configsProvider = installer.Resolve<IConfigsProvider>();
			var dataProvider = installer.Resolve<IDataProvider>();
			var timeService = installer.Resolve<ITimeService>();

			AppLogic = new AppLogic(configsProvider, dataProvider, timeService);
			EntityFactoryLogic = new EntityFactoryLogic(this, configsProvider, dataProvider, timeService);
			CurrencyLogic = new CurrencyLogic(configsProvider, dataProvider, timeService);
			GameplayBoardLogic = new GameplayBoardLogic(configsProvider, dataProvider, timeService);
		}

		/// <inheritdoc />
		public void Init(IDataService dataService, IGameServices gameServices)
		{
			// IMPORTANT: Order of execution is very important in this method

			LoadGameData(dataService, gameServices.TimeService);

			RngLogic = new RngLogic(dataService);

			// ReSharper disable PossibleNullReferenceException
			(CurrencyLogic as IGameLogicInitializer).Init();
			(GameplayBoardLogic as IGameLogicInitializer).Init();

			SetupFirstTimeData(dataService);
		}

		private void LoadGameData(IDataService dataService, ITimeService timeService)
		{
			var time = timeService.DateTimeUtcNow;
			var appData = dataService.LoadData<AppData>();
			var rngData = dataService.LoadData<RngData>();
			var playerData = dataService.LoadData<PlayerData>();

			// First time opens the app
			if (appData.SessionCount == 0)
			{
				var seed = (int)(time.Ticks & int.MaxValue);

				appData.FirstLoginTime = time;
				appData.LoginTime = time;
				rngData.Seed = seed;
				rngData.State = RngService.GenerateRngState(seed);
			}

			appData.SessionCount += 1;
			appData.LastLoginTime = appData.LoginTime;
			appData.LoginTime = time;
		}

		private void SetupFirstTimeData(IDataService dataService)
		{
			if (!dataService.GetData<AppData>().IsFirstSession)
			{
				return;
			}

			GameplayBoardLogic.RefillInputPieces(EntityFactoryLogic.CreatePiece);
			GameplayBoardLogic.RefillBoard(EntityFactoryLogic.CreatePiece, RngLogic);
		}
	}
}