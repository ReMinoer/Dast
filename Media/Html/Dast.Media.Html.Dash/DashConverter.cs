using System;
using System.Collections.Generic;
using Dast.Inputs.Dash;
using Dast.Media.Contracts.Html;
using Dast.Outputs.Html;

namespace Dast.Media.Html.Dash
{
    public class DashConverter : HtmlMediaOutputBase
    {
        private readonly DashInput _dashInput;
        private readonly FragmentedHtmlOutput _fragmentedHtmlOutput;
        private IDictionary<HtmlFragment, string> _fragments;
        
        public override string DisplayName => "Dash document";
        public override MediaType Type => MediaType.Visual;
        public override IEnumerable<FileExtension> FileExtensions { get { yield return Dast.FileExtensions.Text.Dash; } }

        public DashConverter()
        {
            _dashInput = new DashInput();
            _fragmentedHtmlOutput = new FragmentedHtmlOutput();
        }

        public override string Convert(string extension, string content, bool inline)
        {
            var htmlFragments = new []
            {
                HtmlFragment.Body,
                HtmlFragment.Notes
            };

            _fragments = _fragmentedHtmlOutput.Convert(_dashInput.Convert(content), htmlFragments);
            return $"<figure>{Environment.NewLine}{_fragments[HtmlFragment.Body]}{Environment.NewLine}{_fragments[HtmlFragment.Notes]}{Environment.NewLine}</figure>";
        }
    }
}