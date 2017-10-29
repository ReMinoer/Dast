using System.Collections.Generic;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Dash;

namespace Dast.Outputs.Dash
{
    public class DashOutput : ExtensibleFragmentedDocumentMergerBase<FragmentedDashOutput, IDashMediaOutput, DashFragment>
    {
        protected override IEnumerable<DashFragment> MergeFragments()
        {
            yield return DashFragment.Body;
            Writer.WriteLine();
            yield return DashFragment.Notes;
        }
    }
}