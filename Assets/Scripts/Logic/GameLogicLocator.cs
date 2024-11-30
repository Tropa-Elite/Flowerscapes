using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Logic.Shared;
using Game.Logic.Client;
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
	public interface IGameDataProviderLocator
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
		/// <inheritdoc cref="IPiecesDataProvider"/>
		IPiecesDataProvider PieceDataProvider { get; }
	}

	/// <summary>
	/// Provides access to all game's logic
	/// This interface shouldn't be exposed to the views or controllers
	/// To interact with the logic, execute a <see cref="Commands.IGameCommand"/> via the <see cref="ICommandService"/>
	/// </summary>
	public interface IGameLogicLocator : IGameDataProviderLocator
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
		/// <inheritdoc cref="IPiecesLogic"/>
		IPiecesLogic PiecesLogic { get; }
	}

	/// <summary>
	/// This interface provides the contract to initialize the Game Logic
	/// </summary>
	public interface IGameLogicLocatorInit
	{
		/// <summary>
		/// Initializes the Game Logic state to it's default initial values
		/// </summary>
		void Init(IDataService dataService, IGameServicesLocator gameServices);
	}

	/// <inheritdoc cref="IGameLogicLocator"/>
	public class GameLogicLocator : IGameLogicLocator, IGameLogicLocatorInit
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
		public IPiecesDataProvider PieceDataProvider => PiecesLogic;

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
		/// <inheritdoc cref="IPiecesLogic"/>
		public IPiecesLogic PiecesLogic { get; }

		public GameLogicLocator(IInstaller installer)
		{
			var configsProvider = installer.Resolve<IConfigsProvider>();
			var dataProvider = installer.Resolve<IDataProvider>();
			var timeService = installer.Resolve<ITimeService>();

			AppLogic = new AppLogic(this, configsProvider, dataProvider, timeService);
			EntityFactoryLogic = new EntityFactoryLogic(this, configsProvider, dataProvider, timeService);
			CurrencyLogic = new CurrencyLogic(this, configsProvider, dataProvider, timeService);
			GameplayBoardLogic = new GameplayBoardLogic(this, configsProvider, dataProvider, timeService);
			PiecesLogic = new PiecesLogic(this, configsProvider, dataProvider, timeService);
		}

		/// <inheritdoc />
		public void Init(IDataService dataService, IGameServicesLocator gameServices)
		{
			// IMPORTANT: Order of execution is very important in this method

			RngLogic = new RngLogic(dataService);

			// ReSharper disable PossibleNullReferenceException
			(CurrencyLogic as IGameLogicInitializer).Init();
			(GameplayBoardLogic as IGameLogicInitializer).Init();
			(PiecesLogic as IGameLogicInitializer).Init();
		}
	}
}