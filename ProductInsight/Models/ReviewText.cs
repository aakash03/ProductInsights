using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ProductInsight.Models
{
   
    [Table("Results")]
    public class Results
    {
        [Key]
        public int id { get; set; }
        public int rev_id { get; set; }
        public float rep_score { get; set; }
        public string logisticsDept { get; set; }
        public string financeDept { get; set; }
        public string qualityDept { get; set; }
    }


    [Table("ReviewsDetails")]
    public class Reviews
    {
        [Key]
        public int id { get; set; }

        [Required]
        [Display(Name = "AuthToken")]
        public string AuthToken { get; set; }

        [Required]
        [Display(Name = "userID")]
        public string userID { get; set; }

        [Required]
        [Display(Name = "reviewID")]
        public string reviewID { get; set; }

        [Required]
        [Display(Name = "productID`")]
        public string productID { get; set; }

        [Display(Name = "upvotes")]
        public int upvotes { get; set; }

        [Display(Name = "downvotes")]
        public int downvotes { get; set; }

        [Display(Name = "rating")]
        public float rating { get; set; }

        [Required]
        [Display(Name = "reviewerText")]
        public string reviewerText { get; set; }

        public DateTime timestamp { get; set; }

        public bool Status { get; set; }
    }


    public class VotesModel
    {
        [Required]
        [Display(Name = "reviewID")]
        public string reviewID { get; set; }

        [Required]
        [Display(Name = "productID")]
        public string productID { get; set; }

        [Required]
        [Display(Name = "upvotes")]
        public int upvotes { get; set; }

        [Required]
        [Display(Name = "downvotes")]
        public int downvotes { get; set; }

        [Required]
        [Display(Name = "AuthToken")]
        public string AuthToken { get; set; }
    }

    public class Features
    {
        [Key]
        public int id { get; set; }
        public string AuthToken { get; set; }
        public string ProductID { get; set; }
        public string Feature { get; set; }
        public string FeatureScores { get; set; }
    }

    public class AzureDBContext : DbContext
    {
        public DbSet<Features> FeatureSet { get; set; }
        public DbSet<Results> ResultSet { get; set; }
        public DbSet<Reviews> ReviewSet { get; set; }
    }

}