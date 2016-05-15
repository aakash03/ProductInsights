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
using System.Text;using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
        public async Task<ActionResult> ReviewPost( Reviews obj )
        {
             
                if (!ModelState.IsValid)
                {
                    ViewBag.message = "502 Bad Request";
                    return View();
                }

                string ipaddr = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                var User = UserDB.Users.Where(x => x.ApiToken == obj.AuthToken && x.ServerIP == ipaddr );
                if( User.Count() == 0 ) {
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


                string content = "Review,id,upvotes,downvotes,Rating" + Environment.NewLine;
                content += "\"" + reviewText + "\"," + obj.id.ToString() + "," + obj.upvotes.ToString() + "," + obj.downvotes.ToString() + "," + obj.rating.ToString();

                using (var client = new HttpClient())
                {
                    var scoreRequest = new
                    {
                        GlobalParameters = new Dictionary<string, string>() {
                         { "Data", content },
                        }
                    };
                    const string apiKey = "LJwSdl3I0yjg2IF3huyob0iTxFbj6Bawh8q80v5NRFX1DOafg7mSxnjHOZmmzGkphmHbyw3HzZX7Z8AcEo0ZZg=="; // Replace this with the API key for the web service
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);

                    client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/d32c324a164e4554a4a48632e36b5466/services/eeb11fcf45e54160a7501d471a94b0c4/execute?api-version=2.0&details=true");
        
                    HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        ViewBag.message = String.Format("Result: {0}", result);
                    }
                    else
                    {
                        ViewBag.message = string.Format("The request failed with status code: {0}", response.StatusCode);
                        ViewBag.message += Environment.NewLine;
                        // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                        ViewBag.message += response.Headers.ToString();
                        ViewBag.message += Environment.NewLine;
                        string responseContent = await response.Content.ReadAsStringAsync();
                        ViewBag.message = (responseContent);
                    }
                }


                using (var client = new HttpClient())
                {
                    var scoreRequest = new
                    {
                        GlobalParameters = new Dictionary<string, string>() {
                            { "Database query", "select AuthToken,productID,reviewerText from ReviewsDetails where  (productID = 0 and AuthToken = '#FFFF') or (productID = " + obj.productID + " and AuthToken = '" + obj.AuthToken +"');" },
                        }
                    };
                    const string apiKey = "PaIIMjwndjj5k/0gEFd9ImHMlkDWRYLe/mMoZ1V91MXKTXzLxDh2GSk+EamlWBs/377H4i047pN6KiLaMC8sbA=="; // Replace this with the API key for the web service
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/d32c324a164e4554a4a48632e36b5466/services/5deceb5dce37449b954d222fcb704a69/execute?api-version=2.0&details=true");

                    HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        ViewBag.message += String.Format("Result: {0}", result);
                    }
                    else
                    {
                        ViewBag.message += (string.Format("The request failed with status code: {0}", response.StatusCode));

                        // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                        ViewBag.message += (response.Headers.ToString());

                        string responseContent = await response.Content.ReadAsStringAsync();
                        ViewBag.message += (responseContent);
                    }
                }
         
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


                var update = AzureDB.ReviewSet.Where(x => x.reviewID == reviewID && x.productID == productID && x.AuthToken == model.AuthToken );
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
