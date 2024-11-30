using GameLovers.ConfigsProvider;
using GameLovers.Services;

namespace Game.Logic.Shared
{
	/// <summary>
	/// Abstract basic signature for any Logic that is part of the <see cref="IGameLogicLocator"/>
	/// </summary>
	public abstract class AbstractBaseLogic<TData> where TData : class
	{
		protected readonly IGameDataProviderLocator GameDataProvider;
		protected readonly IConfigsProvider ConfigsProvider;
		protected readonly ITimeService TimeService;

		private readonly IDataProvider _dataProvider;

		protected TData Data => _dataProvider.GetData<TData>();

		private AbstractBaseLogic() { }

		public AbstractBaseLogic(
			IGameDataProviderLocator gameDataProvider,
			IConfigsProvider configsProvider,
			IDataProvider dataProvider,
			ITimeService timeService)
		{
			GameDataProvider = gameDataProvider;
			ConfigsProvider = configsProvider;
			_dataProvider = dataProvider;
			TimeService = timeService;
		}
	}
}