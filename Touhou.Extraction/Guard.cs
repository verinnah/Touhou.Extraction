using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Touhou.Extraction;

/// <summary>
/// Provides static methods for guarding conditions. This class cannot be inherited.
/// </summary>
internal static class Guard
{
	/// <summary>
	/// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null"/> or an <see cref="ArgumentException"/> if <paramref name="argument"/> is not writable.
	/// </summary>
	/// <param name="argument">The <see cref="Stream"/> argument to validate as non-<see langword="null"/>.</param>
	/// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds. If you ommit this parameter, the name of <paramref name="argument"/> is used.</param>
	/// <exception cref="ArgumentException"><paramref name="argument"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
	internal static void ThrowIfNullOrNotWritable([NotNull] Stream argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
	{
		ArgumentNullException.ThrowIfNull(argument);

		if (!argument.CanWrite)
		{
			throw new ArgumentException($"{paramName} must be a writable stream.", paramName);
		}
	}

	/// <summary>
	/// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null"/> or an <see cref="ArgumentException"/> if <paramref name="argument"/> is not readable and seekable.
	/// </summary>
	/// <param name="argument">The <see cref="Stream"/> argument to validate as non-<see langword="null"/>.</param>
	/// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds. If you ommit this parameter, the name of <paramref name="argument"/> is used.</param>
	/// <exception cref="ArgumentException"><paramref name="argument"/> is not readable or seekable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
	internal static void ThrowIfNullOrNotReadableAndSeekable([NotNull] Stream argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
	{
		ArgumentNullException.ThrowIfNull(argument);

		if (!argument.CanRead)
		{
			throw new ArgumentException($"{paramName} must be a readable stream.", paramName);
		}

		if (!argument.CanSeek)
		{
			throw new ArgumentException($"{paramName} must be a seekable stream.", paramName);
		}
	}

	/// <summary>
	/// Throws an <see cref="InvalidOperationException"/> if <paramref name="initializationMode"/> is not <see cref="ArchiveInitializationMode.Creation"/>.
	/// </summary>
	/// <param name="initializationMode">The value to validate as equal to <see cref="ArchiveInitializationMode.Creation"/>.</param>
	/// <exception cref="InvalidOperationException"><paramref name="initializationMode"/> is not equal to <see cref="ArchiveInitializationMode.Creation"/>.</exception>
	internal static void ThrowIfNotCreationInitialized(ArchiveInitializationMode initializationMode)
	{
		if (initializationMode != ArchiveInitializationMode.Creation)
		{
			throw new InvalidOperationException("The archive has not been initialized for creation.");
		}
	}

	/// <summary>
	/// Throws an <see cref="InvalidOperationException"/> if <paramref name="initializationMode"/> is not <see cref="ArchiveInitializationMode.Extraction"/>.
	/// </summary>
	/// <param name="initializationMode">The value to validate as equal to <see cref="ArchiveInitializationMode.Extraction"/>.</param>
	/// <exception cref="InvalidOperationException"><paramref name="initializationMode"/> is not equal to <see cref="ArchiveInitializationMode.Extraction"/>.</exception>
	internal static void ThrowIfNotExtractionInitialized(ArchiveInitializationMode initializationMode)
	{
		if (initializationMode != ArchiveInitializationMode.Extraction)
		{
			throw new InvalidOperationException("The archive has not been initialized for extraction.");
		}
	}
}
