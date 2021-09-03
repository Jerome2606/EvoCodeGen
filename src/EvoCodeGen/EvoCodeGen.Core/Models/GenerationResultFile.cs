using System;

namespace EvoCodeGen.Core.Models
{
    public class GenerationResultFile
    {
        public string FilePath { get; set; }
        public int? Position { get; set; }
        public Status FileStatus { get; set; }
        public Exception Exception { get; set; }

        public enum Status
        {
            Created,
            AlreadyExisting,
            CreationFailed
        }
    }
}