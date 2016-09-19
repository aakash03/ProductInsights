using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProductInsight.Models;
using System.IO;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Helpers;
using Newtonsoft.Json;
namespace ProductInsight.Controllers
{
    public class ReviewsController : Controller
    {
        private AzureDBContext AzureDB = new AzureDBContext();
        private ApplicationDbContext UserDB = new ApplicationDbContext();

        // GET: Reviews
        public ActionResult Index()
        {
            return View(AzureDB.ReviewSet.ToList());
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ReviewPost(Reviews obj)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.message = "502 Bad Request";
                return View();
            }

            string ipaddr = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (ipaddr == "::1")
                ipaddr = "127.0.0.1";

            var User = UserDB.Users.Where(x => x.ApiToken == obj.AuthToken && x.ServerIP == ipaddr);
            if (User.Count() == 0)
            {
                ViewBag.message = "Request not Validated";
                return View();
            }


            obj.id = AzureDB.ReviewSet.Count() + 1;
            obj.timestamp = DateTime.Now;

            AzureDB.ReviewSet.Add(obj);
            AzureDB.SaveChanges();

            StreamReader istr = new StreamReader(Server.MapPath("~/App_Data/_stopWords.csv"));
            String pstr = istr.ReadToEnd();

            var arr = pstr.Split(',');

            string punc = "(){}[]^%#$@!*,/;\"?&-";
            string z = "";

            for (int i = 0; i < obj.reviewerText.Length; i++)
                if (!punc.Contains(obj.reviewerText[i]))
                    z += obj.reviewerText[i];

            var rev = z.Split(' ');
            //foreach (string a in arr)
            //  System.Diagnostics.Debug.Write(a);

            string reviewText = "";

            foreach (string s in rev)
            {
                var w = s.ToLower();
                if (!arr.Contains(w))
                    reviewText += w + " ";
            }

            reviewText = "\"" + reviewText + "\"";

            var httpRequest = (HttpWebRequest)WebRequest.Create("http://example.com/mypage/");
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Accept = "application/json";

            //string postData = "id=" + obj.id.ToString() + "&userID=" + obj.userID + "&reviewID=" + obj.reviewID 
            //+ "&productID=" + obj.productID + "&rating=" + obj.rating.ToString() + "&reviewText=" + reviewText;
            //    byte[] dataArray = Encoding.UTF8.GetBytes(postData);

            //    httpRequest.ContentLength = dataArray.Length;

            //    using (Stream requestStream = httpRequest.GetRequestStream())  {
            //        requestStream.Write(dataArray, 0, dataArray.Length);
            //        var webResponse = (HttpWebResponse)httpRequest.GetResponse();
            //    Stream r = webResponse.GetResponseStream();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://192.168.11.125:5000");
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("id", obj.id.ToString()),
                new KeyValuePair<string, string>("userID", obj.userID),
                new KeyValuePair<string, string>("reviewID", obj.reviewID),
                new KeyValuePair<string, string>("productID", obj.productID),
                new KeyValuePair<string, string>("rating", obj.rating.ToString()),
                new KeyValuePair<string, string>("reviewText", reviewText)
            });
                var result = client.PostAsync("/Review/", content).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;


                Results res = JsonConvert.DeserializeObject<Results>(resultContent);
                res.id = AzureDB.ResultSet.Count() + 1;

                AzureDB.ResultSet.Add(res);
            }

            var reviews = AzureDB.ReviewSet.Where(x => x.AuthToken == obj.AuthToken && x.productID == obj.productID).ToList();
            string jsonReviews = JsonConvert.SerializeObject(reviews);

            var c = new HttpClient();
            var resourceAddress = new Uri("http://192.168.11.125:5000/TFIDF/");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage wcfResponse = await c.PostAsync(resourceAddress, new StringContent(jsonReviews, Encoding.UTF8, "application/json"));

            string ResultContent = wcfResponse.Content.ReadAsStringAsync().Result;

            var features = AzureDB.FeatureSet.Where(x => x.AuthToken == obj.AuthToken && x.ProductID == obj.productID);
            foreach (Features feature in features)
            {
                AzureDB.FeatureSet.Remove(feature);
            }

            Features feat = JsonConvert.DeserializeObject<Features>(ResultContent);


            feat.AuthToken = obj.AuthToken;
            feat.ProductID = obj.productID;
            AzureDB.FeatureSet.Add(feat);

            AzureDB.SaveChanges();

            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpvotesDownvotes(VotesModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.message = "Parameters Not Found";
                    return View();
                }

                string ipaddr = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                var User = UserDB.Users.Where(x => x.ApiToken == model.AuthToken && x.ServerIP == ipaddr);
                if (User.Count() == 0)
                {
                    ViewBag.message = "Request not Validated";
                    return View();
                }

                string reviewID = model.reviewID;
                string productID = model.productID;
                int upvotes = model.upvotes;
                int downvotes = model.downvotes;


                var update = AzureDB.ReviewSet.Where(x => x.reviewID == reviewID && x.productID == productID && x.AuthToken == model.AuthToken);
                if (update.Count() == 0)
                {
                    ViewBag.message = "Invalid ReviewID or ProductID";
                    return View();
                }

                var idx = update.First();

                idx.upvotes += upvotes;
                idx.downvotes += downvotes;
                idx.Status = true;
                AzureDB.Entry(idx).State = EntityState.Modified;
                AzureDB.SaveChanges();
                ViewBag.message = "Updated Successfully";

                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }


        // GET: Reviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reviews reviews = AzureDB.ReviewSet.Find(id);
            if (reviews == null)
            {
                return HttpNotFound();
            }
            return View(reviews);
        }

        // GET: Reviews/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,reviewID,productID,userID,rating,reviewerText,timestamp,upvotes,downvotes")] Reviews reviews)
        {
            if (ModelState.IsValid)
            {
                AzureDB.ReviewSet.Add(reviews);
                AzureDB.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(reviews);
        }

        // GET: Reviews/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reviews reviews = AzureDB.ReviewSet.Find(id);
            if (reviews == null)
            {
                return HttpNotFound();
            }
            return View(reviews);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,reviewID,productID,userID,rating,reviewerText,timestamp,upvotes,downvotes")] Reviews reviews)
        {
            if (ModelState.IsValid)
            {
                AzureDB.Entry(reviews).State = EntityState.Modified;
                AzureDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(reviews);
        }

        // GET: Reviews/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reviews reviews = AzureDB.ReviewSet.Find(id);
            if (reviews == null)
            {
                return HttpNotFound();
            }
            return View(reviews);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reviews reviews = AzureDB.ReviewSet.Find(id);
            AzureDB.ReviewSet.Remove(reviews);
            AzureDB.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AzureDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
