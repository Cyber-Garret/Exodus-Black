using System.ComponentModel.DataAnnotations;

namespace Neiralink.Enums
{
    public enum LangKey
    {
        /// <summary>
        /// English locale
        /// </summary>
        [Display(Name = "English")]
        EN,
        /// <summary>
        /// Russian locale
        /// </summary>
        [Display(Name = "Russian")]
        RU,
        /// <summary>
        /// Ukrainian locale
        /// </summary>
        [Display(Name = "Ukrainian")]
        UK
    }
}
