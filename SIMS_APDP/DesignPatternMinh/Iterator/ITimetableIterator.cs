using SIMS_APDP.Models;

namespace SIMS_APDP.DesignPatternMinh.Iterator
{
    public interface ITimetableIterator
    {
        bool HasNext();
        Timetable Next();
    }
}