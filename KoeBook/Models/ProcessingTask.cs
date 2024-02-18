using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoeBook.Models;

public class ProcessingTask
{
    public ProcessingTask()
    {
        Id = Guid.NewGuid();
    }

    public ProcessingTask(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
