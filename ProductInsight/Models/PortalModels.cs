using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
namespace ProductInsight.Models
{
    public class ManageModel
    {
        [Required]
        [RegularExpression(@"((www\.|(http|https|ftp|news|file|)+\:\/\/)?[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])",ErrorMessage="Enter Valid Website")]
        [Display(Name = "Website")]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",ErrorMessage="Enter Valid IP address")]
        [Display(Name = "Server IP")]
        public string ServerIP { get; set; }

    }

    public class UserDetailsSettings
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

    }
    public class ChangePasswordSettings
    {
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Old Password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name="New Password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
 
    }

    public class InsightsModel
    {
        public string id { get; set; }
        public double score { get; set; }
        public int positive { get; set; }
        public int negative { get; set; }
        public List<Reviews> RecentReview { get; set; }
        public List<string> QualityDept { get; set; }
        public List<string> FinanceDept { get; set; }
        public List<string> LogisticDept { get; set; }
        public SortedDictionary<DateTime, int> LineChart { get; set; }

        public class Pair
        {
            public string feature { get; set; }
            public double score { get; set; }
            public Pair( string p, double q )
            { feature = p; score = q; }
        }

        public List<Pair> Features { get; set; }

    }

    public class ReviewModel
    {
        public Reviews Review { get; set; }
        public Results Result { get; set; }
    }


    public class OverviewModel
    {
        public List<ReviewModel> val { get; set; }
        public int positive { get; set; }
        public int negative { get; set; }
        public List<string> QualityDept { get; set; }
        public List<string> FinanceDept { get; set; }
        public List<string> LogisticDept { get; set; }
       
    }
    public class SettingsModel
    {
        public UserDetailsSettings User { get; set; }
        public ChangePasswordSettings Pass { get; set; }
    }

    public class DashboardModel
    {
        public int Reviews { get; set; }
        public int Products { get; set; }
        public int Positive { get; set; }
        public int Negative { get; set; }
        public SortedDictionary<DateTime, int> AreaChart { get; set; }

        public int QualityDept { get; set; }
        public int FinanceDept { get; set; }
        public int LogisticDept { get; set; }
        public int Others { get; set; }

        public class Pair
        {
            public string pID { get; set; }
            public double score { get; set; }
        }
        public List<Pair> ProductList { get; set; }

    }

}