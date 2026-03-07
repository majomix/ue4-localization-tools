using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UE4TextConverter.Model
{
    internal class BatchConverter
    {
        public const string SourceMarkerPrefix = "@@@SOURCE: ";

        private readonly bool _compact;
        private readonly bool _sortById;

        public BatchConverter(bool compact, bool sortById)
        {
            _compact = compact;
            _sortById = sortById;
        }

        /// <summary>
        /// Traverses localizationRoot, reads .locres files from the specified
        /// language subdirectory in each category, writes a single consolidated TXT.
        /// </summary>
        public void BatchExport(string localizationRoot, string outputTxtPath, string language)
        {
            if (!Directory.Exists(localizationRoot))
            {
                Console.Error.WriteLine($"Error: Localization root directory not found: {localizationRoot}");
                return;
            }

            var validFileCount = 0;

            using (var fileStream = File.Open(outputTxtPath, FileMode.Create))
            using (var writer = new StreamWriter(fileStream, Encoding.Unicode))
            {
                var categoryDirs = Directory.GetDirectories(localizationRoot);
                Array.Sort(categoryDirs, StringComparer.OrdinalIgnoreCase);

                foreach (var categoryDir in categoryDirs)
                {
                    var categoryName = Path.GetFileName(categoryDir);
                    var langDir = Path.Combine(categoryDir, language);

                    if (!Directory.Exists(langDir))
                    {
                        Console.Error.WriteLine($"Warning: Category '{categoryName}' does not contain language directory '{language}', skipping.");
                        continue;
                    }

                    var locresFiles = Directory.GetFiles(langDir, "*.locres");
                    Array.Sort(locresFiles, StringComparer.OrdinalIgnoreCase);

                    foreach (var locresFile in locresFiles)
                    {
                        var relativePath = GetRelativePath(localizationRoot, locresFile);
                        var tempFile = Path.GetTempFileName();

                        try
                        {
                            var converter = new TextConverter();
                            converter.LoadLocresFile(locresFile);
                            converter.WriteTextFile(tempFile, _compact, _sortById);

                            // Write source marker
                            writer.WriteLine(SourceMarkerPrefix + relativePath);

                            // Append the temp TXT content to the consolidated file
                            using (var tempStream = File.Open(tempFile, FileMode.Open))
                            using (var reader = new StreamReader(tempStream, Encoding.Unicode))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    writer.WriteLine(line);
                                }
                            }

                            validFileCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error: Failed to read '{locresFile}': {ex.Message}");
                        }
                        finally
                        {
                            if (File.Exists(tempFile))
                            {
                                try { File.Delete(tempFile); } catch { /* best effort cleanup */ }
                            }
                        }
                    }
                }
            }

            if (validFileCount == 0)
            {
                Console.Error.WriteLine($"Warning: No valid .locres files found for language '{language}'.");
            }
        }

        /// <summary>
        /// Reads a consolidated TXT file, splits by source markers,
        /// recreates .locres files in the output directory.
        /// </summary>
        public void BatchImport(string consolidatedTxtPath, string outputDir)
        {
            if (!File.Exists(consolidatedTxtPath))
            {
                Console.Error.WriteLine($"Error: Consolidated TXT file not found: {consolidatedTxtPath}");
                return;
            }

            string currentRelativePath = null;
            var currentSegmentLines = new List<string>();

            using (var fileStream = File.Open(consolidatedTxtPath, FileMode.Open))
            using (var reader = new StreamReader(fileStream, Encoding.Unicode))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(SourceMarkerPrefix))
                    {
                        // Flush previous segment
                        if (currentRelativePath != null)
                        {
                            WriteLocresSegment(currentSegmentLines, currentRelativePath, outputDir);
                        }

                        // Parse new marker
                        var relativePath = line.Substring(SourceMarkerPrefix.Length);
                        if (IsValidSourceMarkerPath(relativePath))
                        {
                            currentRelativePath = relativePath;
                            currentSegmentLines.Clear();
                        }
                        else
                        {
                            Console.Error.WriteLine($"Error: Invalid source marker path: {line}");
                            currentRelativePath = null;
                            currentSegmentLines.Clear();
                        }
                    }
                    else if (currentRelativePath != null)
                    {
                        currentSegmentLines.Add(line);
                    }
                    // If currentRelativePath is null (invalid marker), skip lines until next valid marker
                }

                // Flush last segment
                if (currentRelativePath != null)
                {
                    WriteLocresSegment(currentSegmentLines, currentRelativePath, outputDir);
                }
            }
        }

        /// <summary>
        /// Writes a single .locres file from a text segment.
        /// </summary>
        private void WriteLocresSegment(List<string> lines, string relativePath, string outputDir)
        {
            if (lines.Count == 0)
                return;

            // Write segment to a temp file so TextConverter.LoadTextFile can read it
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var tempStream = File.Open(tempFile, FileMode.Create))
                using (var writer = new StreamWriter(tempStream, Encoding.Unicode))
                {
                    foreach (var line in lines)
                    {
                        writer.WriteLine(line);
                    }
                }

                var converter = new TextConverter();
                converter.LoadTextFile(tempFile);

                // Convert forward slashes in relative path to OS path separators
                var osRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                var outputPath = Path.Combine(outputDir, osRelativePath);

                // Create necessary subdirectories
                var outputDirectory = Path.GetDirectoryName(outputPath);
                if (outputDirectory != null && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                converter.WriteLocresFile(outputPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: Failed to import segment for '{relativePath}': {ex.Message}");
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* best effort cleanup */ }
                }
            }
        }

        /// <summary>
        /// Validates that a source marker path is a reasonable relative path.
        /// Invalid paths include: empty, absolute paths, paths with '..' traversal,
        /// or paths containing invalid characters.
        /// </summary>
        private static bool IsValidSourceMarkerPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Must not be an absolute path
            if (Path.IsPathRooted(path.Replace('/', Path.DirectorySeparatorChar)))
                return false;

            // Must not contain parent directory traversal
            var segments = path.Split('/');
            if (segments.Any(s => s == ".."))
                return false;

            // Must end with .locres
            if (!path.EndsWith(".locres", StringComparison.OrdinalIgnoreCase))
                return false;

            // Check for invalid path characters
            try
            {
                var osPath = path.Replace('/', Path.DirectorySeparatorChar);
                // This will throw if the path contains invalid characters
                Path.GetFullPath(Path.Combine(Path.GetTempPath(), osPath));
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Computes a relative path from a base directory to a target file,
        /// using forward slashes as separators.
        /// </summary>
        private static string GetRelativePath(string basePath, string targetPath)
        {
            // Normalize paths
            var baseUri = new Uri(EnsureTrailingSlash(Path.GetFullPath(basePath)));
            var targetUri = new Uri(Path.GetFullPath(targetPath));
            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        private static string EnsureTrailingSlash(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }
    }
}
