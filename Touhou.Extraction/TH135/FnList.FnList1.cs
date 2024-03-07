namespace Touhou.Extraction.TH135;

internal abstract partial class FnList
{
	private sealed class FnList1 : FnList
	{
		/// <inheritdoc/>
		protected override uint GetFileNameHashCore(string fileName, uint initialHash = 0x811C9DC5u)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

			uint ch;
			uint hash = initialHash;

			for (int i = 0; i < fileName.Length; hash = (hash ^ ch) * 0x1000193)
			{
				char c = fileName[i++];
				ch = c;

				if (char.IsAscii(c))
				{
					ch = char.ToLowerInvariant(c);

					if (ch == '/')
					{
						ch = '\\';
					}
				}
			}

			return (uint)-(int)hash;
		}
	}
}
