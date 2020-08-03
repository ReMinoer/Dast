using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Dast.Extensibility;
using Dast.Inputs.Dash;
using Dast.Media.Contracts.Html;
using Dast.Outputs.Html;

namespace Dast.Media.Html.Dash
{
    public class DashConverter : HtmlMediaOutputBase, IExtensible<IHtmlMediaOutput>
    {
        private Lazy<IEnumerable<IHtmlMediaOutput>> _htmlMediaOutputs;

        public override string DisplayName => "Dash document";
        public override MediaType Type => MediaType.Visual;
        public override IEnumerable<FileExtension> FileExtensions { get { yield return Dast.FileExtensions.Text.Dash; } }

        public override string Convert(string extension, string content, bool inline) => Convert(extension, content, inline, out _);
        public override string Convert(string extension, string content, bool inline, out IHtmlMediaOutput[] usedMediaOutputs)
        {
            var dashInput = new DashInput();
            var fragmentedHtmlOutput = new FragmentedHtmlOutput();
            fragmentedHtmlOutput.MediaCatalog.AddRange(_htmlMediaOutputs.Value);

            var htmlFragments = new[]
            {
                HtmlFragment.Body,
                HtmlFragment.Notes
            };

            IDictionary<HtmlFragment, string> fragments = fragmentedHtmlOutput.Convert(dashInput.Convert(content), htmlFragments);

            usedMediaOutputs = fragmentedHtmlOutput.UsedMediaConverters.ToArray();
            return $"<figure>{Environment.NewLine}{fragments[HtmlFragment.Body]}{Environment.NewLine}{fragments[HtmlFragment.Notes]}{Environment.NewLine}</figure>";
        }

        public ICollection<IHtmlMediaOutput> Extensions => _htmlMediaOutputs.Value.ToArray();
        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);

        public IEnumerable<IHtmlMediaOutput> Extend(CompositionContext context)
        {
            _htmlMediaOutputs = new Lazy<IEnumerable<IHtmlMediaOutput>>(context.GetExports<IHtmlMediaOutput>);
            return Enumerable.Empty<IHtmlMediaOutput>();
        }

        public void ResetToVanilla()
        {
            _htmlMediaOutputs = null;
        }
    }
}