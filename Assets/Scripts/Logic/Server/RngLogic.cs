using GameLovers;
using GameLovers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Logic.Shared
{
	public interface IRngDataProvider
	{
		/// <summary>
		/// The <see cref="IRngData"/> that this service is manipulating
		/// </summary>
		public IRngData Data { get; }

		/// <summary>
		/// Returns the number of times the Rng has been counted;
		/// </summary>
		int Counter { get; }

		/// <summary>
		/// Requests the next <see cref="int"/> generated value without changing the state.
		/// Calling this multiple times in sequence gives always the same result.
		/// </summary>
		int Peek { get; }

		/// <summary>
		/// Requests the next <see cref="float"/> generated value without changing the state.
		/// Calling this multiple times in sequence gives always the same result.
		/// </summary>
		floatP Peekfloat { get; }

		int PeekRange(int min, int max, bool maxInclusive = false);

		floatP PeekRange(floatP min, floatP max, bool maxInclusive = true);
	}

	public interface IRngLogic : IRngService, IRngDataProvider { }

	public class RngLogic : RngService, IRngLogic
	{
		public RngLogic(IDataProvider dataProvider) : base(dataProvider.GetData<RngData>())
		{
		}

	}
}
