using System.ComponentModel;

namespace Domain.Models.Job
{
    public enum JobStatus
    {
        [Description("В обработке")]
        InProcessing,

        [Description("Завершено")]
        Completed,

        [Description("Ошибка")]
        Error,

        [Description("Не запущено")]
        NotStarted,
    }
}