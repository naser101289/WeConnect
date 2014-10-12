using System;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using WebApplication1.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private ProfileContext db = new ProfileContext();
        private ApplicationDbContext UserDB = new ApplicationDbContext();

        // GET: /Profile/
        public ActionResult Index()
        { 
            var currentUser = UserDB.Users.Find(User.Identity.GetUserId());
            if (currentUser.MadeProfileYet)
            {   
                var vm = new ProfileStatusVM();
                vm.StatusList = new List<Status>();
                
                Guid UserId = new Guid(User.Identity.GetUserId());
                var profile = db.Profile.FirstOrDefault(x => x.AspNetUser_Id == UserId);
                vm.Profile = profile;
                var listOfAllStatuses = db.Status.ToList();
                var listOfUserStatus = (from x in listOfAllStatuses
                                        where x.UserWhomStatusBelongsTo == profile.Id
                                        select x).ToList();

                foreach (var entry in listOfUserStatus)
                    vm.StatusList.Add(entry);
                if (vm.StatusList.Count == 0)
                {
                    Status firstStatus = new Status();
                    firstStatus.StatusUpdate = "Welcome!";
                    firstStatus.TimeOfUpdate = DateTime.Now;
                    firstStatus.UserWhomStatusBelongsTo = profile.Id;
                    firstStatus.UpdatedByFullName = "Admin";
                    db.Status.Add(firstStatus);
                    vm.StatusList.Add(firstStatus);
                    db.SaveChanges();
                    //vm.StatusList.Add(new Status { StatusUpdate = "Welcome!", UserWhomStatusBelongsTo = profile.Id, UpdatedByFullName = profile.FullName, TimeOfUpdate = DateTime.Now });
                }
                
                return View(vm);
            }            
            else
                return RedirectToAction("MakeProfile");
        }

        //GET
        public ActionResult MakeProfile()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakeProfile([Bind(Include="Id, FirstName, LastName")] Profile profile, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ImageFile.InputStream.CopyTo(ms);
                        profile.Picture = ms.GetBuffer();
                    }
                }
                profile.Id = Guid.NewGuid();
                db.Profile.Add(profile);
                db.SaveChanges();

                var currentUser = UserDB.Users.Find(User.Identity.GetUserId());
                currentUser.MadeProfileYet = true;
                UserDB.SaveChanges();
                
                db.Profile.Find(profile.Id).AspNetUser_Id = new Guid(currentUser.Id);
                db.Profile.Find(profile.Id).FavoriteSaying = UserDB.Users.Find(currentUser.Id).FavoritePhrase;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(profile);
        }

        public ActionResult GetImage(Guid id)
        {
            byte[] imageData = db.Profile.Find(id).Picture;
            return File(imageData, "image/jpeg");
        }

        //GET
        public ActionResult EditStatus(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profile.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(id);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStatus(Guid? id, string statusUpdate)
        {
            if (id != null && !String.IsNullOrWhiteSpace(statusUpdate))
            {
                Status Update = new Status();
                var user = db.Profile.Find(id);
                Update.StatusUpdate = statusUpdate;                
                Update.UpdatedByFullName = user.FullName;
                Update.UserWhomStatusBelongsTo = user.Id;
                Update.TimeOfUpdate = DateTime.Now;

                db.Status.Add(Update);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(id);
        }

        //GET
        public ActionResult EditImage(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profile.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(id);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditImage(Guid? id, HttpPostedFileBase imageFile)
        {
            if (id != null && imageFile != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    imageFile.InputStream.CopyTo(ms);
                    db.Profile.Find(id).Picture = ms.GetBuffer();
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(id);
        }

        //GET
        public ActionResult ShowFriends(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profile.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            var vm = new ProfileStatusVM();
            vm.StatusList = new List<Status>();
            vm.ProfileCollection = new List<Profile>();
            vm.ProfileCollection = db.Profile.ToList();
            vm.ProfileCollection.Remove(profile);
            vm.StatusList = db.Status.ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveMessage(Guid FriendId, string MsgForFriend)
        {
            var currentUserId = new Guid(User.Identity.GetUserId());
            Profile currentUserProfile = db.Profile.FirstOrDefault(x => x.AspNetUser_Id == currentUserId);
            Profile profileOfFriend = db.Profile.FirstOrDefault(x => x.Id == FriendId);
            Status statusUpdate = new Status();

            if (String.IsNullOrWhiteSpace(MsgForFriend))
                return RedirectToAction("ShowFriends", new { id = currentUserProfile.Id });
            statusUpdate.StatusUpdate = MsgForFriend;
            statusUpdate.UpdatedByFullName = currentUserProfile.FullName;
            statusUpdate.UserWhomStatusBelongsTo = profileOfFriend.Id;
            statusUpdate.TimeOfUpdate = DateTime.Now;
            db.Status.Add(statusUpdate);
            db.SaveChanges();
            return RedirectToAction("ShowFriends", new { id = currentUserProfile.Id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                UserDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
