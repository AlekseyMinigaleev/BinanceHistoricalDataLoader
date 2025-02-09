
namespace Domain.Exceptions
{
    public class JobInterruptedException : Exception
    {
        public JobInterruptedException() : base("The execution of the task was unexpectedly interrupted.") { }

        public JobInterruptedException(string message) : base(message) { }
    }
}
