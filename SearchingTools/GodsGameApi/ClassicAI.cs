using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodsGameApi
{
	public class ClassicAI
	{
		internal int maxRecursionDepth = 4;

		ClassicGameState state;

		ClassicRules rules = new ClassicRules();

		internal ClassicAI(ClassicGameState state)
		{
			this.state = state;
		}
		
		/// <summary>
		/// Соотносит текущие жизни игроков
		/// </summary>
		/// <param name="left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		double GetHpRatio(Player left, Player right)
		{
			int leftHp = left.Hp.Current;
			int rightHp = right.Hp.Current;
			if (rightHp <= 0)
				return 1000 * 1000;
			if (leftHp <= 0)
				return -1000 * 1000;

			return (leftHp + 15) / (double)(rightHp + 15);
		}
		
		/// <summary>
		/// Соотносит текущие жизни игроков в игре state
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		double GetCurrentHpRatio(ClassicGameState state)
		{
			return GetHpRatio(state.CurrentPlayer, state.AnotherPlayer);
		}

		/// <summary>
		/// Получает один из лучших (возможно не самых) ходов для CurrentPlayer
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public Movement GetGoodMovement(int maxRecursionDepth, CancellationToken ct)
		{
			this.maxRecursionDepth = maxRecursionDepth;
			int temp = 0;
			return GetGoodMovementImpl(state, 1, ref temp, ct).Key;
		}

		/// <summary>
		/// Получает один из лучших (возможно не самых) ходов для CurrentPlayer
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public Movement GetGoodMovement(int maxRecursionDepth)
		{
			this.maxRecursionDepth = maxRecursionDepth;
			int temp = 0;
			return GetGoodMovementImpl(state, 1, ref temp, CancellationToken.None).Key;
		}

		private KeyValuePair<Movement, ClassicGameState> GetGoodMovementImpl(ClassicGameState state, 
			int depth, ref int switches, CancellationToken ct)
		{
			// Все ходы CurrentPlayer. В этих состояниях CurrentPlayer == Enemy
			var map = rules.GetAllMovements(state);
			
			if (map.Count == 0)
				return new KeyValuePair<Movement, ClassicGameState>(null, state);

			var result = new KeyValuePair<Movement,ClassicGameState>();

			if (depth == maxRecursionDepth)
			{
				// Наихудший ход для Enemy
				GetBestState(map, -1, ref result);
			}
			else
			{
				var resultArray = new KeyValuePair<Movement, ClassicGameState>[map.Count];
				
				// Тут храним количество смен игроков. Так как игра может закончится
				// внезапно, формула, использующая maxRecursionDepth не годится
				var resultSwitches = new int[map.Count];

				ct.ThrowIfCancellationRequested();

				if (depth == 1) // запускаем параллельный алгоритм
				{
					Parallel.ForEach(map, (pair, loopState, iteration) =>
						{
							resultArray[iteration] = new KeyValuePair<Movement,ClassicGameState>(
								pair.Key, GetGoodMovementImpl(pair.Value, depth + 1, ref resultSwitches[iteration], ct).Value);
						});
				}
				else
				{
					int index = 0;
					foreach (var pair in map)
					{
						resultArray[index] = new KeyValuePair<Movement,ClassicGameState>(pair.Key,
							GetGoodMovementImpl(pair.Value, depth + 1, ref resultSwitches[index], ct).Value);
						++index;
					}
				}

				var max = double.NegativeInfinity;
				int minSwitches = int.MaxValue;
				for (int i = 0; i < resultArray.Length; ++i)
				{
					int mult = resultSwitches[i] % 2 == 0 ? -1 : 1;
					var currHpRatio = GetCurrentHpRatio(resultArray[i].Value) * mult;
					if (currHpRatio > max || currHpRatio > 10000 &&
						resultSwitches[i] < minSwitches) 
					{
						minSwitches = resultSwitches[i];
						switches = resultSwitches[i];
						max = mult * GetCurrentHpRatio(resultArray[i].Value);
						result = resultArray[i];
					}
				} 
			}

			++switches;
			return result;
		}

		/// <summary>
		/// Находит лучшее состояние игры для нашего игрока
		/// </summary>
		/// <param name="map"></param>
		/// <param name="comparisonMultiplier"></param>
		/// <param name="result"></param>
		private void GetBestState(IEnumerable<KeyValuePair<Movement, ClassicGameState>> map, 
			int comparisonMultiplier, ref KeyValuePair<Movement, ClassicGameState> result)
		{
			var max = double.NegativeInfinity;

			foreach (var pair in map)
			{
				double value = GetCurrentHpRatio(pair.Value) * comparisonMultiplier;
				if (value > max)
				{
					max = value;
					result = pair;
				}
			}
		}

	}
}
