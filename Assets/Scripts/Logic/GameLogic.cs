using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Logic.Shared;

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
		/// <inheritdoc cref="ICurrencyLogic"/>
		ICurrencyLogic CurrencyLogic { get; }
		/// <inheritdoc cref="IGameplayBoardLogic"/>
		IGameplayBoardLogic GameplayBoardLogic { get; }
	}

	/// <inheritdoc cref="IGameLogic"/>
	public interface IGameLogicInit : IGameLogic, IGameLogicInitializer
	{
	}

	/// <inheritdoc cref="IGameLogic"/>
	public class GameLogic : IGameLogicInit
	{
		/// <inheritdoc />
		public IAppDataProvider AppDataProvider => AppLogic;
		/// <inheritdoc />
		public ICurrencyDataProvider CurrencyDataProvider => CurrencyLogic;
		/// <inheritdoc />
		public IGameplayBoardDataProvider GameplayBoardDataProvider => GameplayBoardLogic;

		/// <inheritdoc />
		public IAppLogic AppLogic { get; }
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
			CurrencyLogic = new CurrencyLogic(configsProvider, dataProvider, timeService);
			GameplayBoardLogic = new GameplayBoardLogic(configsProvider, dataProvider, timeService);
		}

		/// <inheritdoc />
		public void Init()
		{
			// ReSharper disable PossibleNullReferenceException
			
			(CurrencyLogic as IGameLogicInitializer).Init();
		}
	}
}