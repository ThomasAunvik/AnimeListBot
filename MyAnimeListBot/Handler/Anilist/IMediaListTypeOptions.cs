using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaListTypeOptions
    {
        List<string> sectionOrder { get; }

        bool splitCompletedSectionFormat { get; }

        List<string> customLists { get; }
        List<string> advancedSorting { get; }

        bool advancedScoringEnabled { get; }
    }
}
