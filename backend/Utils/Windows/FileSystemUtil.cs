namespace Utils.Windows
{
    public class FileSystemUtil
    {
        /// <summary>
        /// Clears a directory of all files and folders
        /// </summary>
        public static void ClearDirectory(string path)
        {
            var di = new DirectoryInfo(path);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        /// <summary>
        /// returns true if given directory is empty.
        /// </summary>
        public static bool DirectoryIsEmpty(string path)
        {
            var di = new DirectoryInfo(path);
            return !di.EnumerateFiles().Any() && !di.EnumerateDirectories().Any();
        }

        /// <summary>
        /// Returns true if string contains illegale file name charater
        /// </summary>
        public static bool ContainsIllegalFileNameChars(string input)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return input.IndexOfAny(invalidChars) >= 0;
        }

        /// <summary>
        /// Returns true if string contains illegale file path charater
        /// </summary>
        public static bool ContainsIllegalPathChars(string input)
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            return input.IndexOfAny(invalidChars) >= 0;
        }
    }
}