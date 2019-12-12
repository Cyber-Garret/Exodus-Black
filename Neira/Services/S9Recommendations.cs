using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Services
{
	public class S9Recommendations : AbstractRecommendations
	{
		protected override int SoftCap => 900;
		protected override int PowerfulCap => 960;
		protected override int HardCap => 970;
	}
}
