using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Dast.Extensibility;
using Dast.Inputs.Dash;
using Dast.Media.Contracts.Markdown;
using Dast.Outputs.GitHubMarkdown;

namespace Dast.Media.Markdown.Dash
{
    public class DashConverter : IMarkdownMediaOutput, IExtensible<IMarkdownMediaOutput>
    {
        private Lazy<IEnumerable<IMarkdownMediaOutput>> _markdownMediaOutputs;
        
        public string DisplayName => "Dash document";
        public MediaType Type => MediaType.Visual;
        public IEnumerable<FileExtension> FileExtensions { get { yield return Dast.FileExtensions.Text.Dash; } }

        public string Convert(string extension, string content, bool inline)
        {
            var dashInput = new DashInput();
            var fragmentedGitHubMarkdownOutput = new FragmentedGitHubMarkdownOutput();
            fragmentedGitHubMarkdownOutput.MediaCatalog.AddRange(_markdownMediaOutputs.Value);

            var githubMarkdownFragments = new []
            {
                GithubMarkdownFragment.Body,
                GithubMarkdownFragment.Notes
            };

            IDictionary<GithubMarkdownFragment, string> fragments = fragmentedGitHubMarkdownOutput.Convert(dashInput.Convert(content), githubMarkdownFragments);
            return $"{fragments[GithubMarkdownFragment.Body]}{Environment.NewLine}{fragments[GithubMarkdownFragment.Notes]}";
        }

        public ICollection<IMarkdownMediaOutput> Extensions => _markdownMediaOutputs.Value.ToArray();
        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);

        public IEnumerable<IMarkdownMediaOutput> Extend(CompositionContext context)
        {
            _markdownMediaOutputs = new Lazy<IEnumerable<IMarkdownMediaOutput>>(context.GetExports<IMarkdownMediaOutput>);
            return Enumerable.Empty<IMarkdownMediaOutput>();
        }

        public void ResetToVanilla()
        {
            _markdownMediaOutputs = null;
        }
    }
}