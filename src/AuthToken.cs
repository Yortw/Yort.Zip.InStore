using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	internal class AuthToken
	{
		public string? access_token { get; set; }
		public long expires_in { get; set; }
		public string? scope { get; set; }
		public string? token_type { get; set; }

		public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

		public bool IsExpired()
		{
			return DateTimeOffset.UtcNow >= CreatedAt.AddSeconds(Math.Max(1, expires_in - 10));
		}
	}
}
