using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductInsight.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ProductInsight.Controllers
{

    [AllowAnonymous]
    public class PortalController : Controller
    {
        private ApplicationUserManager _userManager;
        private AzureDBContext AzureDB = new AzureDBContext();
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: Portal
        public ActionResult Index()
        {

            DashboardModel model = new DashboardModel();
            model.AreaChart = new SortedDictionary<DateTime, int>();

            var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var curUser = manager.FindById(User.Identity.GetUserId());

            List<Reviews> Review = AzureDB.ReviewSet.Where(x => x.AuthToken == curUser.ApiToken).ToList();
            List<int> RevId = new List<int>();
            List<string> pId = new List<string>();
            Dictionary<string,List<int>> ProductDict = new Dictionary<string,List<int>>();

            foreach (Reviews p in Review)
            {
                RevId.Add(p.id);
                if (!model.AreaChart.ContainsKey(p.timestamp.Date))
                    model.AreaChart.Add(p.timestamp.Date, 1);
                else
                    model.AreaChart[p.timestamp.Date]++;
                if (!pId.Contains(p.productID)) {
                    pId.Add(p.productID);
                    ProductDict.Add(p.productID, new List<int>());
                }

                ProductDict[p.productID].Add(p.id);
            }

            model.Reviews = RevId.Count();
            model.Products = pId.Count();

            List<Results> ResultSet = AzureDB.ResultSet.Where(x => RevId.Contains(x.rev_id)).ToList();

            model.Positive = ResultSet.Where(x => RevId.Contains(x.rev_id) && x.rep_score > 2.5).ToList().Count();
            model.Negative = ResultSet.Where(x => RevId.Contains(x.rev_id) && x.rep_score < 2.5).ToList().Count();
            model.LogisticDept = ResultSet.Where(x => RevId.Contains(x.rev_id) && x.logisticsDept != "0").ToList().Count();
            model.QualityDept = ResultSet.Where(x => RevId.Contains(x.rev_id) && x.qualityDept != "0").ToList().Count();
            model.FinanceDept = ResultSet.Where(x => RevId.Contains(x.rev_id) && x.financeDept != "0").ToList().Count();

            model.ProductList = new List<DashboardModel.Pair>();
            foreach (string p in pId)
            {
                double score = 0;
                int count = 0;
                foreach (var q in ProductDict[p])
                {
                    Reviews Rev = Review.Where(x => x.id == q).ToList().First();
                    Results Res = ResultSet.Where(x => x.rev_id == q).ToList().First();

                    if (Rev.downvotes > Rev.upvotes + 5)
                        continue;
                    count++;
                    score += Res.rep_score;          
                }

                DashboardModel.Pair c = new DashboardModel.Pair();
                c.pID = p; c.score = Math.Round(score/count,2);
                model.ProductList.Add(c);
             
            }
            
            
            return View(model);
        }

        public ActionResult Manage() {
            ManageModel obj = new ManageModel();
          if (User.Identity.IsAuthenticated)
            {
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(userStore);
                var curUser = manager.FindById(User.Identity.GetUserId());
                obj.Website = curUser.Website;
                obj.ServerIP = curUser.ServerIP;
            }
            return View(obj);
        }

        [HttpPost]
        public async Task<ActionResult> Manage(ManageModel obj)
        {
            if(!ModelState.IsValid) return View(obj);

   
            var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var curUser = manager.FindById(User.Identity.GetUserId());
            curUser.Website = obj.Website;
            curUser.ServerIP = obj.ServerIP;
            var result = await manager.UpdateAsync(curUser);
            await userStore.Context.SaveChangesAsync();

            ModelState.AddModelError("", "Data updated successfully!");     
            return View(obj);
        }


        public ActionResult Settings()
        {
         
            SettingsModel obj = new SettingsModel();
            obj.User = new UserDetailsSettings();
            obj.Pass = new ChangePasswordSettings();

            var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var curUser = manager.FindById(User.Identity.GetUserId());

            obj.User.FirstName = curUser.FirstName;
            obj.User.LastName = curUser.LastName;
            obj.User.Phone = curUser.Phone;

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Settings( SettingsModel mod )
        {
            if (!ModelState.IsValid) return View();
            if(String.IsNullOrEmpty(mod.Pass.OldPassword))
            {
                var obj = mod.User;
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(userStore);
                var curUser = manager.FindById(User.Identity.GetUserId());
                curUser.FirstName = obj.FirstName;
                curUser.LastName = obj.LastName;
                curUser.Phone = obj.Phone;
                var result = await manager.UpdateAsync(curUser);
                await userStore.Context.SaveChangesAsync();

                ModelState.AddModelError("", "Data updated successfully!");
                return Settings();
            }
            else
            {
                var model = mod.Pass;
                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    ModelState.AddModelError("", "Password updated successfully!");
                    return Settings();
     
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect Old Password");
                    return Settings();
                }
               
         
            }
            
        }

        public string ProcessReview(string str, string logistics, string quality, string finance)
        {
            string[] sentence = str.Split('.');
            string final = "";
            string[] logList = logistics.Split(',');
            string[] qualList = quality.Split(',');
            string[] finList = finance.Split(',');

            for (int i = 0; i < sentence.Length; i++)
            {
                string idx = (i+1).ToString();
                if (logList.Contains(idx))
                    final += "<span title='Logistics' class='logistics'>" + sentence[i] + ".</span>";
                else if (quality.Contains(idx))
                    final += "<span title='Quality' class='quality'>" + sentence[i] + ".</span>";
                else if (finList.Contains(idx))
                    final += "<span title='Finance' class='finance'>" + sentence[i] + ".</span>";
                else
                    final += sentence[i] + ".";
            }

            return final;
        }

        public ActionResult DisplayInsights( string id  )
        {
            InsightsModel model = new ProductInsight.Models.InsightsModel();
            model.id = id; model.positive = model.negative = 0;

            model.RecentReview = new List<Models.Reviews>();
            model.LogisticDept = new List<string>();
            model.FinanceDept = new List<string>();
            model.QualityDept = new List<string>();
            model.LineChart = new SortedDictionary<DateTime, int>();

            var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var curUser = manager.FindById(User.Identity.GetUserId());
          

            ViewBag.message = id;
            List<Reviews> rev = AzureDB.ReviewSet.Where(x => x.productID == id && x.AuthToken == curUser.ApiToken ).ToList();

            if (rev.Count() == 0)
                return Content("<div class='alert-danger alert'>The Product ID you searched for cannot be found in our database. Please recheck!</div>");

           List<Results> idx = new List<Results>();
            rev = rev.OrderByDescending(x => x.timestamp).ToList();
            
            int count = 0;

            foreach (Reviews p in rev)
            {
                if (p.downvotes > p.upvotes + 5) continue;
         
                Results Res = AzureDB.ResultSet.Where(x => x.rev_id == p.id).ToList().First();
                p.reviewerText = ProcessReview(p.reviewerText, Res.logisticsDept, Res.qualityDept, Res.financeDept);
               
                if (Res.rep_score > 2.5) p.Status = true;
                else p.Status = false;

                model.RecentReview.Add(p);

                if (Res.rep_score >= 2.5)
                    model.positive++;
                else
                    model.negative++;
                if (Res.logisticsDept != "0")
                    model.LogisticDept.Add(p.reviewerText);
                if (Res.qualityDept != "0")
                    model.QualityDept.Add(p.reviewerText);
                if (Res.financeDept != "0")
                    model.FinanceDept.Add(p.reviewerText);
              
                if (!model.LineChart.ContainsKey(p.timestamp.Date))
                    model.LineChart.Add(p.timestamp.Date, 1);
                else
                    model.LineChart[p.timestamp.Date]++;
      
                count++;
                model.score += Res.rep_score;
            }

            AzureDBContext FeatDB = new AzureDBContext();
            Features ft = FeatDB.FeatureSet.Where(x => x.ProductID == id && x.AuthToken == curUser.ApiToken ).ToList().First();

            string[] features = ft.Feature.Split(',');
            string[] scores = ft.FeatureScores.Split(',');

            model.Features = new List<InsightsModel.Pair>();
            for (int i = 0; i < features.Length; i++)
                 model.Features.Add(new InsightsModel.Pair(features[i], Convert.ToDouble(scores[i])));
            
            


            model.score /= count;
            return PartialView("ProductView",model);
        }

        public ActionResult OverviewSearch( string ProductID = "ALL", double RatingL = 0, double RatingR = 5, double ScoreL = -5, double ScoreR = 5, int Upvotes = 0, int downvotes = 0 )
        {
            OverviewModel model = new Models.OverviewModel();
            model.val = new List<Models.ReviewModel>();
            model.positive = model.negative = 0;
            model.LogisticDept = new List<string>();
            model.FinanceDept = new List<string>();
            model.QualityDept = new List<string>();
          

            List<Reviews> Query = new List<Reviews>();

            var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var curUser = manager.FindById(User.Identity.GetUserId());


            if (ProductID.Contains("ALL"))
                Query = AzureDB.ReviewSet.Where(x => x.AuthToken == curUser.ApiToken && x.upvotes >= Upvotes && x.downvotes >= downvotes).ToList();
            else
            {
                List<string> pid = ProductID.Split(',').ToList();
                Query = AzureDB.ReviewSet.Where(x => x.AuthToken == curUser.ApiToken && pid.Contains(x.productID) && x.upvotes >= Upvotes && x.downvotes >= downvotes).ToList();
            }

            if (Query.Count() == 0)
                return Content("<div class='alert-danger alert'>Your search returned no results!</div>");


            Query.OrderByDescending(x => x.timestamp );
  
            foreach (Reviews p in Query)
            {
                Results Res = AzureDB.ResultSet.Where(x => x.rev_id == p.id).ToList().First();
                p.reviewerText = ProcessReview(p.reviewerText, Res.logisticsDept, Res.qualityDept, Res.financeDept);
        
                if (Res.rep_score > 2.5) p.Status = true;
                else p.Status = false;

                if (Res.rep_score >= 2.5)
                    model.positive++;
                else
                    model.negative++;
                if (Res.logisticsDept != "0")
                    model.LogisticDept.Add(p.reviewerText);
                if (Res.qualityDept != "0")
                    model.QualityDept.Add(p.reviewerText);
                if (Res.financeDept != "0")
                    model.FinanceDept.Add(p.reviewerText);
              
                ReviewModel q = new ReviewModel();
                q.Review = p;
                q.Result = Res;

                model.val.Add(q);

          
            }
            
            return PartialView("SearchView", model);
     
        }

        public ActionResult Overview()
        {
            return View();
        }

        public ActionResult Insights()
        {
            return View();
        }
        
        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}