using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OzCodeReview
{
    internal partial class OptionsProvider
    {
        // Register the options with these attributes on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.HiddenOptions), "OzCodeReview.Options", "Hidden", 0, 0, true)]
        // [ProvideProfile(typeof(OptionsProvider.HiddenOptions), "OzCodeReview.Options", "Hidden", 0, 0, true)]
        [ComVisible(true)]
        public class HiddenOptions : BaseOptionPage<Hidden> { }
    }

    public class Hidden : BaseOptionModel<Hidden>
    {
        public string Token { get; set; }
    }
}
