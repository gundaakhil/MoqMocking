using System;

namespace Loans.Domain.Applications
{
    public interface INowProvider
    {
        DateTime GetNow();
    }
}
