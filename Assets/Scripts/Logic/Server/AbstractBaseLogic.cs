using GameLovers.ConfigsProvider;
using GameLovers.Services;

namespace Game.Logic.Shared
{
	/// <summary>
	/// Abstract basic signature for any Logic that is part of the <see cref="IGameLogicLocator"/>
	/// </summary>
	public abstract class AbstractBaseLogic<TData> where TData : class
	{
		protected readonly IGameLogicLocator GameLogic;
		protected readonly IConfigsProvider ConfigsProvider;
		protected readonly ITimeService TimeService;

		private readonly IDataProvider _dataProvider;

		protected TData Data => _dataProvider.GetData<TData>();

		private AbstractBaseLogic() { }

		public AbstractBaseLogic(
			IGameLogicLocator gameLogic,
			IConfigsProvider configsProvider,
			IDataProvider dataProvider,
			ITimeService timeService)
		{
			GameLogic = gameLogic;
			ConfigsProvider = configsProvider;
			_dataProvider = dataProvider;
			TimeService = timeService;
		}
	}
}