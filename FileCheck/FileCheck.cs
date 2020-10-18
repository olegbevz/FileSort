using System.Reflection;
using FileSort.Core;
using log4net;

namespace FileCheck
{
    /// <summary>
    /// FileGenerate contains random file check logic
    /// </summary>
    public class FileCheck
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _fileBuffer;
        private readonly int _streamBuffer;
        private readonly bool _onlyCheckFormat;

        public FileCheck(int fileBuffer, int streamBuffer, bool onlyCheckFormat)
        {
            _fileBuffer = fileBuffer;
            _streamBuffer = streamBuffer;
            _onlyCheckFormat = onlyCheckFormat;
        }

        public bool Check(string fileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(fileName, _fileBuffer))
            {
                FileLine previousLine = FileLine.None;
                bool firstLineReaden = false;
                foreach (var fileLine in new FileLineReader(fileStream, _streamBuffer))
                {
                    if (!_onlyCheckFormat && firstLineReaden)
                    {
                        if (previousLine.CompareTo(fileLine) > 0)
                        {
                            _logger.Warn($"File '{fileName}' is not properly sorted.");
                            _logger.Warn($"Line '{fileLine}' should be before line '{previousLine}'.");
                            return false;
                        }
                    }

                    previousLine = fileLine;
                    firstLineReaden = true;
                }

                return true;
            }
        }
    }
}
