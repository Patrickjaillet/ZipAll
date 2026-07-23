using System.IO.Compression;

namespace ZipAll.Core;

public static class ArchiveVerifier
{
    public static VerificationResult Verify(string zipPath, int expectedEntryCount)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var actualCount = 0;
            var buffer = new byte[81920];

            foreach (var entry in archive.Entries)
            {
                using var entryStream = entry.Open();
                int bytesRead;
                do
                {
                    bytesRead = entryStream.Read(buffer, 0, buffer.Length);
                }
                while (bytesRead > 0);

                actualCount++;
            }

            if (actualCount != expectedEntryCount)
            {
                return new VerificationResult(false, actualCount, expectedEntryCount,
                    $"Expected {expectedEntryCount} entries but found {actualCount}.");
            }

            return new VerificationResult(true, actualCount, expectedEntryCount, null);
        }
        catch (InvalidDataException ex)
        {
            return new VerificationResult(false, 0, expectedEntryCount, ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new VerificationResult(false, 0, expectedEntryCount, ex.Message);
        }
    }
}
