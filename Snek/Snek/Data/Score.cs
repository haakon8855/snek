﻿namespace Snek.Data
{
	public class Score
	{
		public int Id { get; set; }

		public required int Points { get; set; }

		public required ApplicationUser User { get; set; }

	}
}
