using System;
using System.Collections.Generic;
using Dast.Inputs.Dash;
using Dast.Media.Contracts.Markdown;
using Dast.Outputs.GitHubMarkdown;

namespace Dast.Media.Markdown.Dash
{
    public class DashConverter : IMarkdownMediaOutput
    {
        private readonly DashInput _dashInput;
        private readonly FragmentedGitHubMarkdownOutput _fragmentedGitHubMarkdownOutput;
        private IDictionary<GithubMarkdownFragment, string> _fragments;
        
        public string DisplayName => "Dash document";
        public MediaType Type => MediaType.Visual;
        public IEnumerable<FileExtension> FileExtensions { get { yield return Dast.FileExtensions.Text.Dash; } }

        public DashConverter()
        {
            _dashInput = new DashInput();
            _fragmentedGitHubMarkdownOutput = new FragmentedGitHubMarkdownOutput();
        }

        public string Convert(string extension, string content, bool inline)
        {
            var githubMarkdownFragments = new []
            {
                GithubMarkdownFragment.Body,
                GithubMarkdownFragment.Notes
            };

            _fragments = _fragmentedGitHubMarkdownOutput.Convert(_dashInput.Convert(content), githubMarkdownFragments);
            return $"{_fragments[GithubMarkdownFragment.Body]}{Environment.NewLine}{_fragments[GithubMarkdownFragment.Notes]}";
        }
    }
}