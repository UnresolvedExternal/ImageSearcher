using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NUnit.Framework;
using System.Collections;

namespace GodsGameApi
{
	[TestFixture]
	class ClassicAITest
	{
		[Test, TestCaseSource(typeof(TestCaseSourceClass), "SimpleTests")]
		public void Test(string board, ClassicMovement bestMovement, int recursionDepth)
		{
			var state = TestHelper.CreateState(board, 50, 50, 0, 0, 0, 0);
			var Ai = new ClassicAI(state);
			Ai.maxRecursionDepth = recursionDepth;
			var movement = Ai.GetGoodMovement(Ai.maxRecursionDepth);

			Assert.That(bestMovement, Is.EqualTo(movement));
		}
	}

	class TestCaseSourceClass
	{
		public static IEnumerable SimpleTests
		{
			get
			{
				var board = @"
YYBYHHPGY
YYBRYYGHH
RHPGYRPBH
GYGBRYRBB
YRBYYGGYR
HPGBYGGDH
BHPHPPHGG";
				var bestMovement = new ClassicMovement
				{
					First = new Point(6, 3),
					Second = new Point(6, 4),
					Kind = ClassicMovementKind.Swap
				};

				yield return new TestCaseData(board, bestMovement, 1).SetName("OneMoveTest");

				board = @"
YYBYUHPGY
YYBRUYGHH
RHPGURPBH
GYGBUYRBB
YRBYUGGYR
HPGBUGGDH
BHPHUPHGG";

				bestMovement = new ClassicMovement
				{
					First = new Point(3, 5),
					Second = new Point(4, 5),
					Kind = ClassicMovementKind.Swap
				};

				yield return new TestCaseData(board, bestMovement, 1).SetName("OneMoveTestU");

				board = @"
YYBYHHPGY
YYBRYYGHH
RHPGYRPBH
GYGBRYRBB
YRBYYGGYR
HPGBYGGDH
BHPHPPHGG";
				
				bestMovement = new ClassicMovement
				{
					First = new Point(6, 3),
					Second = new Point(6, 4),
					Kind = ClassicMovementKind.Swap
				};

				yield return new TestCaseData(board, bestMovement, 2).SetName("TwoMoveTest");

				board = @"
YYBYHHPGY
YYBRYYGHH
RHPGYRPBH
GYGBRYRBB
YRBYYGGYR
HPGBYGGDH
BHPHPPHGG";

				bestMovement = new ClassicMovement
				{
					First = new Point(5, 4),
					Second = new Point(6, 4),
					Kind = ClassicMovementKind.Swap
				};

				yield return new TestCaseData(board, bestMovement, 3).SetName("ThreeMoveDynRulesTest");

				board = @"
YYBYHHPGY
YYBRYYGHH
RHPGYRPBH
GYGBRYRBB
YRBYYGGYR
HPGBYGGDH
BHPHPPHGG";

				bestMovement = new ClassicMovement
				{
					First = new Point(5, 4),
					Second = new Point(6, 4),
					Kind = ClassicMovementKind.Swap
				};

				yield return new TestCaseData(board, bestMovement, 4).SetName("FourMoveDynRulesTest");
			}
		}

	}

}
