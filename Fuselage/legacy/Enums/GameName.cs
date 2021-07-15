using System.ComponentModel.DataAnnotations;

namespace Neiralink.Enums
{
    public enum GameName : byte
    {
        /// <summary>
        /// Destiny 2
        /// </summary>
        [Display(Name = "Destiny 2")]
        Destiny2,
        /// <summary>
        /// The Division 2
        /// </summary>
        [Display(Name = "The Division 2")]
        Division2,
        /// <summary>
        /// CoD: Warzone
        /// </summary>
        [Display(Name = "CoD: Warzone")]
        Warzone
    }
}
