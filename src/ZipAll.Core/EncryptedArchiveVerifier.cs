using ICSharpCode.SharpZipLib.Zip;

namespace ZipAll.Core;

public static class EncryptedArchiveVerifier
{
    public static VerificationResult Verify(string zipPath, string password, int expectedEntryCount)
    {
        try
        {
            using var archive = new ZipFile(zipPath) { Password = password };
            var actualCount = 0;
            var buffer = new byte[81920];

            foreach (ZipEntry entry in archive)
            {
                if (!entry.IsFile)
                {
                    continue;
                }

                using var entryStream = archive.GetInputStream(entry);
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
        catch (Exception ex) when (ex is ZipException or IOException or UnauthorizedAccessException)
        {
            return new VerificationResult(false, 0, expectedEntryCount, ex.Message);
        }
    }
}
