// DesignPatternMinh/Iterator/StudentFeaturesIterator.cs
using SIMS_APDP.DesignPatternMinh.Factory;

namespace SIMS_APDP.DesignPatternMinh.Iterator
{
    public interface IFeatureIterator
    {
        bool HasNext();
        IStudentPage Next();
    }

    public class StudentFeaturesIterator : IFeatureIterator
    {
        private readonly List<IStudentPage> _features = new()
        {
            new TableTimePage(),
            new TranscriptPage(),
            new FeedbackPage()
        };
        private int _current = 0;

        public bool HasNext() => _current < _features.Count;
        public IStudentPage Next() => _features[_current++];
    }
}