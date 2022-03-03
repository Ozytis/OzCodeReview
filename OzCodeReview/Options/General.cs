using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OzCodeReview
{
    internal partial class OptionsProvider
    {
        // Register the options with these attributes on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.GeneralOptionsOptions), "OzCodeReview", "GeneralOptions", 0, 0, true)]
        // [ProvideProfile(typeof(OptionsProvider.GeneralOptionsOptions), "OzCodeReview", "GeneralOptions", 0, 0, true)]
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("Connection")]
        [DisplayName("ServerUrl")]
        [Description("Base OzCodeReview server URL")]
        [DefaultValue("")]
        public string ServerUrl { get; set; }

        [Category("Connection")]
        [DisplayName("Email")]
        public string Email { get; set; }

        [Category("Connection")]
        [DisplayName("Password")]
        [PasswordPropertyText(true)]
        public string Password { get; set; }
    }
}
