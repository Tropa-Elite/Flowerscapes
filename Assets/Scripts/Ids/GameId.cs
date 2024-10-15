using System.Collections.Generic;
using System.Collections.ObjectModel;

/* AUTO GENERATED CODE */
namespace Game.Ids
{
	public enum GameId
	{
		Random,
		SoftCurrency,
		HardCurrency,
		Flower,
		Petal
	}

	public enum GameIdGroup
	{
		Random,
		Currency,
		Flower
	}

	public static class GameIdLookup
	{
		public static bool IsInGroup(this GameId id, GameIdGroup group)
		{
			if (!_groups.TryGetValue(id, out var groups))
			{
				return false;
			}
			return groups.Contains(group);
		}

		public static IList<GameId> GetIds(this GameIdGroup group)
		{
			return _ids[group];
		}

		public static IList<GameIdGroup> GetGroups(this GameId id)
		{
			return _groups[id];
		}

		public class GameIdComparer : IEqualityComparer<GameId>
		{
			public bool Equals(GameId x, GameId y)
			{
				return x == y;
			}

			public int GetHashCode(GameId obj)
			{
				return (int)obj;
			}
		}

		public class GameIdGroupComparer : IEqualityComparer<GameIdGroup>
		{
			public bool Equals(GameIdGroup x, GameIdGroup y)
			{
				return x == y;
			}

			public int GetHashCode(GameIdGroup obj)
			{
				return (int)obj;
			}
		}

		private static readonly Dictionary<GameId, ReadOnlyCollection<GameIdGroup>> _groups =
			new Dictionary<GameId, ReadOnlyCollection<GameIdGroup>> (new GameIdComparer())
			{
				{
					GameId.Random, new List<GameIdGroup>
					{
						GameIdGroup.Random
					}.AsReadOnly()
				},
				{
					GameId.SoftCurrency, new List<GameIdGroup>
					{
						GameIdGroup.Currency
					}.AsReadOnly()
				},
				{
					GameId.HardCurrency, new List<GameIdGroup>
					{
						GameIdGroup.Currency
					}.AsReadOnly()
				},
				{
					GameId.Flower, new List<GameIdGroup>
					{
						GameIdGroup.Flower
					}.AsReadOnly()
				},
				{
					GameId.Petal, new List<GameIdGroup>
					{
						GameIdGroup.Flower
					}.AsReadOnly()
				},
			};

		private static readonly Dictionary<GameIdGroup, ReadOnlyCollection<GameId>> _ids =
			new Dictionary<GameIdGroup, ReadOnlyCollection<GameId>> (new GameIdGroupComparer())
			{
				{
					GameIdGroup.Random, new List<GameId>
					{
						GameId.Random
					}.AsReadOnly()
				},
				{
					GameIdGroup.Currency, new List<GameId>
					{
						GameId.SoftCurrency,
						GameId.HardCurrency
					}.AsReadOnly()
				},
				{
					GameIdGroup.Flower, new List<GameId>
					{
						GameId.Flower,
						GameId.Petal
					}.AsReadOnly()
				},
			};
	}
}
