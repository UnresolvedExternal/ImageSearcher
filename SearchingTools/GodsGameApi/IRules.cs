using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	interface IRules
	{
		Dictionary<Movement, ClassicGameState> GetAllMovements(ClassicGameState state);
	}
}
