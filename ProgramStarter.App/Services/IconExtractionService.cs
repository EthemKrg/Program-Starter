using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProgramStarter.App.Services;

/// <summary>
/// Extracts and caches icons from executable files for display in app cards.
/// Falls back to a Kaldforge placeholder icon if extraction fails.
/// </summary>
public static class IconExtractionService
{
    private static readonly ConcurrentDictionary<string, ImageSource?> IconCache = new();

    /// <summary>
    /// Gets the icon for the specified executable path.
    /// Results are cached in memory keyed by the full path.
    /// Returns null if extraction fails (caller should fallback to Kaldforge icon).
    /// This method is thread-safe.
    /// </summary>
    public static ImageSource? GetIcon(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        // Normalize path separators for cache consistency
        var normalizedPath = Path.GetFullPath(filePath).ToUpperInvariant();

        return IconCache.GetOrAdd(normalizedPath, ExtractIconFromPath);
    }

    /// <summary>
    /// Clears the in-memory icon cache. Call this if executable paths change.
    /// </summary>
    public static void ClearCache()
    {
        IconCache.Clear();
    }

    /// <summary>
    /// Removes a specific path from the icon cache.
    /// </summary>
    public static void InvalidatePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        var normalizedPath = Path.GetFullPath(filePath).ToUpperInvariant();
        IconCache.TryRemove(normalizedPath, out _);
    }

    private static ImageSource? ExtractIconFromPath(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            if (icon is null)
                return null;

            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            try
            {
                var imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                imageSource.Freeze();
                return imageSource;
            }
            finally
            {
                // Free the unmanaged GDI handle
                NativeMethods.DeleteObject(hBitmap);
            }
        }
        catch
        {
            // Extraction failed — caller will use fallback
            return null;
        }
    }
}

/// <summary>
/// Native methods for GDI handle cleanup.
/// </summary>
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(IntPtr hObject);
}
