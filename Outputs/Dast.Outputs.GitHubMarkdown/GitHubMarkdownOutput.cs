using System.Collections.Generic;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Markdown;

namespace Dast.Outputs.GitHubMarkdown
{
    public class GitHubMarkdownOutput : ExtensibleFragmentedDocumentMergerBase<FragmentedGitHubMarkdownOutput, IMarkdownMediaOutput, GithubMarkdownFragment>
    {
        protected override IEnumerable<GithubMarkdownFragment> MergeFragments()
        {
            yield return GithubMarkdownFragment.Body;
            Writer.WriteLine();
            yield return GithubMarkdownFragment.Notes;
        }
    }
}