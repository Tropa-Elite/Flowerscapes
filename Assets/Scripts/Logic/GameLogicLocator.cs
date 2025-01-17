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
        /// <inheritdoc cref="ITileBoardDataProvider"/>
        ITileBoardDataProvider TileBoardDataProvider { get; }
        /// <inheritdoc cref="IDeckSpawnerDataProvider"/>
        IDeckSpawnerDataProvider DeckSpawnerDataProvider { get; }
        /// <inheritdoc cref="IPiecesDataProvider"/>
        IPiecesDataProvider PieceDataProvider { get; }
        /// <inheritdoc cref="IGameLevelDataProvider"/>
        IGameLevelDataProvider GameLevelDataProvider { get; }
    }

    /// <summary>
    /// Provides access to all game's logic
    /// This interface shouldn't be exposed to the views or controllers
    /// To interact with the logic, execute a <see cref="IGameCommand{TGameLogic}"/> via the <see cref="ICommandService"/>
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
        /// <inheritdoc cref="ITileBoardLogic"/>
        ITileBoardLogic TileBoardLogic { get; }
        /// <inheritdoc cref="IDeckSpawnerLogic"/>
        IDeckSpawnerLogic DeckSpawnerLogic { get; }
        /// <inheritdoc cref="IPiecesLogic"/>
        IPiecesLogic PiecesLogic { get; }
        /// <inheritdoc cref="IGameLevelLogic"/>
        IGameLevelLogic GameLevelLogic { get; }
    }

    /// <summary>
    /// This interface provides the contract to initialize the Game Logic
    /// </summary>
    public interface IGameLogicLocatorInit : IGameLogicLocator
    {
        /// <summary>
        /// Initializes the Game Logic state to it's default initial values
        /// </summary>
        void Init(IDataService dataService, IGameServicesLocator gameServices);
    }

    /// <inheritdoc cref="IGameLogicLocator"/>
    public class GameLogicLocator : IGameLogicLocatorInit
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
        public ITileBoardDataProvider TileBoardDataProvider => TileBoardLogic;
        /// <inheritdoc />
        public IDeckSpawnerDataProvider DeckSpawnerDataProvider => DeckSpawnerLogic;
        /// <inheritdoc />
        public IPiecesDataProvider PieceDataProvider => PiecesLogic;
        /// <inheritdoc />
        public IGameLevelDataProvider GameLevelDataProvider => GameLevelLogic;

        /// <inheritdoc />
        public IAppLogic AppLogic { get; }
        /// <inheritdoc />
        public IRngLogic RngLogic { get; private set; }
        /// <inheritdoc />
        public IEntityFactoryLogic EntityFactoryLogic { get; }
        /// <inheritdoc />
        public ICurrencyLogic CurrencyLogic { get; }
        /// <inheritdoc />
        public ITileBoardLogic TileBoardLogic { get; }
        /// <inheritdoc />
        public IDeckSpawnerLogic DeckSpawnerLogic { get; }
        /// <inheritdoc />
        public IPiecesLogic PiecesLogic { get; }
        /// <inheritdoc />
        public IGameLevelLogic GameLevelLogic { get; }

        public GameLogicLocator(IInstaller installer)
        {
            var configsProvider = installer.Resolve<IConfigsProvider>();
            var dataProvider = installer.Resolve<IDataProvider>();
            var timeService = installer.Resolve<ITimeService>();

            AppLogic = new AppLogic(this, configsProvider, dataProvider, timeService);
            EntityFactoryLogic = new EntityFactoryLogic(this, configsProvider, dataProvider, timeService);
            CurrencyLogic = new CurrencyLogic(this, configsProvider, dataProvider, timeService);
            TileBoardLogic = new TileBoardLogic(this, configsProvider, dataProvider, timeService);
            DeckSpawnerLogic = new DeckSpawnerLogic(this, configsProvider, dataProvider, timeService);
            PiecesLogic = new PiecesLogic(this, configsProvider, dataProvider, timeService);
            GameLevelLogic = new GameLevelLogic(this, configsProvider, dataProvider, timeService);
        }

        /// <inheritdoc />
        public void Init(IDataService dataService, IGameServicesLocator gameServices)
        {
            // IMPORTANT: Order of execution is very important in this method

            RngLogic = new RngLogic(dataService);

            // ReSharper disable PossibleNullReferenceException
            (GameLevelLogic as IGameLogicInitializer).Init();
            (CurrencyLogic as IGameLogicInitializer).Init();
            (DeckSpawnerLogic as IGameLogicInitializer).Init();
            (PiecesLogic as IGameLogicInitializer).Init();
        }
    }
}