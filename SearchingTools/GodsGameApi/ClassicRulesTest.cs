using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	[TestFixture]
	class ClassicRulesTest
	{
		private IRules rules = new ClassicRules();

		[Test]
		public void SingleSwapTest()
		{
			var before = @"
RGB
BGR
GRR";

			var stateBefore = TestHelper.CreateState(before, 90, 90, 0, 0, 0, 0);

			var after = new[] 
			{@"
RUB
BUR
RUR"
			};

			var movements = new[] 
			{
				TestHelper.CreateMovementEmpty(),
				TestHelper.CreateMovementSwap(1, 3, 2, 3)
			};

			var statesAfter = new List<ClassicGameState>();
			statesAfter.Add(TestHelper.CreateState(before, 90, 90 , 0, 0, 0, 0));
			statesAfter.Add(TestHelper.CreateState(after[0], 81, 90, 0, 0, 0, 0));

			var answer = rules.GetAllMovements(stateBefore);
			var rightAnswer = TestHelper.CreateDictionary(movements, statesAfter);

			Assert.That(answer, Is.EqualTo(rightAnswer));
		}


		[Test]
		public void TripleSwapTest()
		{
			var before = @"
RGB
RGR
GRR";

			var stateBefore = TestHelper.CreateState(before, 90, 90, 0, 0, 0, 0);

			var after = new[] 
			{@"
UUB
UUR
UUR", 

@"
UUU
RGB
GGR",

@"
UUU
RGB
GGR"
			};

			var movements = new[] 
			{
				TestHelper.CreateMovementEmpty(),
				TestHelper.CreateMovementSwap(1, 3, 2, 3),
				TestHelper.CreateMovementSwap(1, 2, 1, 3),
				TestHelper.CreateMovementSwap(2, 2, 2, 3)
			};

			var statesAfter = new List<ClassicGameState>();
			statesAfter.Add(TestHelper.CreateState(before, 90, 90, 0, 0, 0, 0));
			statesAfter.Add(TestHelper.CreateState(after[0], 90 - 24, 90, 0, 0, 0, 0));
			statesAfter.Add(TestHelper.CreateState(after[1], 90 - 15, 90, 0, 0, 0, 0));
			statesAfter.Add(TestHelper.CreateState(after[2], 90 - 15, 90, 0, 0, 0, 0));

			var answer = rules.GetAllMovements(stateBefore);
			var rightAnswer = TestHelper.CreateDictionary(movements, statesAfter);
			
			Assert.That(answer, Is.EqualTo(rightAnswer));
		}


		[Test]
		public void SingleSwapFullExplosiveTest()
		{
			var before = @"
RGB
BGR
GRR";

			var stateBefore = TestHelper.CreateState(before, 25, 25, 2, 8, 5, 3);

			var after = new[] 
			{@"
RUB
BUR
RUR",

//Бомба
@"
UUB
UGR
GRR",

@"
UUU
BUR
GRR",

@"
RUU
BGU
GRR",

@"
UUB
UGR
URR",

@"
UUU
RUB
GUR",

@"
RUU
BGU
GRU",

@"
UUB
UGR
RGR",

@"
UUU
RUB
BGR",

@"
RUU
BGU
GGB",

// Динамит
@"
UUB
UUR
GRR",

@"
UUU
UUU
GRR",

@"
RUU
BUU
GRR",

@"
UUB
UUR
UUR",

@"
UUU
UUU
UUU",

@"
RUU
BUU
GUU",

@"
UUB
UUR
RGR",

@"
UUU
UUU
RGB",

@"
RUU
BUU
GGB"
			};

			var movements = new[] 
			{
				TestHelper.CreateMovementEmpty(),
				TestHelper.CreateMovementSwap(1, 3, 2, 3)
			};

			for (int y = 1; y <= 3; ++y)
				for (int x = 1; x <= 3; ++x)
					movements = movements.Concat(new[] {TestHelper.CreateMovementBomb(x, y)}).ToArray();

			for (int y = 1; y <= 3; ++y)
				for (int x = 1; x <= 3; ++x)
					movements = movements.Concat(new[] { TestHelper.CreateMovementDynamit(x, y) }).ToArray();

			var statesAfter = new List<ClassicGameState>();
			statesAfter.Add(TestHelper.CreateState(before, 25, 25, 8, 2, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[0], 25 - 9, 25, 8, 2, 3, 5));

			statesAfter.Add(TestHelper.CreateState(after[1], 25 - 14, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[2], 25 - 17, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[3], 25 - 14, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[4], 25 - 17, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[5], 25 - 22, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[6], 25 - 19, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[7], 25 - 14, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[8], 25 - 18, 25, 8, 1, 3, 5));
			statesAfter.Add(TestHelper.CreateState(after[9], 25 - 17, 25, 8, 1, 3, 5));

			statesAfter.Add(TestHelper.CreateState(after[10], 25 - 18, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[11], 25 - 25, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[12], 25 - 18, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[13], 25 - 25, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[14], 25 - 25, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[15], 25 - 25, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[16], 25 - 18, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[17], 25 - 25, 25, 8, 2, 3, 4));
			statesAfter.Add(TestHelper.CreateState(after[18], 25 - 21, 25, 8, 2, 3, 4));

			var answer = rules.GetAllMovements(stateBefore);
			var rightAnswer = TestHelper.CreateDictionary(movements, statesAfter);

			var union = answer.Union(rightAnswer).ToList();
			var intersection = answer.Intersect(rightAnswer).ToList();
			var difference = union.Except(intersection).ToList();

			Assert.That(answer, Is.EqualTo(rightAnswer));
		}

		[Test]
		public void BigBoardCustomCheckingTest()
		{
			var before = @"
YYBYHHPGY
YYBRYYGHH
RHPGYRPBH
GYGBRYRBB
YRBYYGGYR
HPGBYGGDH
BHPHPPHGG";

			var stateBefore = TestHelper.CreateState(before, 25, 25, 2, 2, 2, 2);

			var after = new[] {@"
YYBYUHPGY
YYBRUYGHH
RHPGURPBH
GYGBURRBB
YRBYUGGYR
HPGBHGGDH
BHPHPPHGG",
			
@"
YYBYHHUUU
YYBRYYPUY
RHPGYRGGH
GYGBRYPHH
YRBYYGRBB
HPGBYGGBR
BHPHPPHYH"};

			var movements = new[] 
			{
				TestHelper.CreateMovementSwap(5, 4, 6, 4),
				TestHelper.CreateMovementSwap(7, 6, 7, 7)
			};

			var statesAfter = new List<ClassicGameState>();
			statesAfter.Add(TestHelper.CreateState(after[0], 25 - 10, 25, 2, 2, 2, 3));
			statesAfter.Add(TestHelper.CreateState(after[1], 25 - 9, 25, 2, 2, 2, 2));

			var answer = rules.GetAllMovements(stateBefore);

			for (int i = 0; i < movements.Length; ++i)
				Assert.That(answer.Contains(new KeyValuePair<Movement, ClassicGameState>(movements[i], statesAfter[i])));

			Assert.That(answer.Where(pair => ((ClassicMovement)pair.Key).Kind == ClassicMovementKind.Swap).Count(), Is.EqualTo(14));
			Assert.That(answer.Where(pair => ((ClassicMovement)pair.Key).Kind == ClassicMovementKind.Bomb).Count(), Is.EqualTo(9 * 7));
			Assert.That(answer.Where(pair => ((ClassicMovement)pair.Key).Kind == ClassicMovementKind.Dynamit).Count(), Is.EqualTo(9 * 7));
		}
	}

}
