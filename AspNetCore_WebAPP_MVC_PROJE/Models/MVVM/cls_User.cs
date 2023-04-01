using AspNetCore_WebAPP_MVC_PROJE.Models.DbSets;
using Microsoft.EntityFrameworkCore;
using System.Text;
using XSystem.Security.Cryptography;

namespace AspNetCore_WebAPP_MVC_PROJE.Models.MVVM
{
    public class cls_User
    {
        KayaliContext context = new KayaliContext();

        //ADMIN PANEL LOGIN CONTROL
        public async Task<User> LoginControl(User user)
        {
            User? usr = await context.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == user.Password && u.IsAdmin == true && u.Active == true);
            return usr;
        }

        //REGULAR USER LOGIN CONTROL
        public User? SelectUserInfo(string userInfo)
        {
            User? user = context.Users.FirstOrDefault(u => u.Email == userInfo);
            return user;
        }

        public string MemberControl(User user)
        {
            string answer = "";

            try
            {
                string? md5Password = MD5PassConverter(user.Password);

                User? usr = context.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == md5Password);

                if (usr == null)
                {
                    //Wrong password or mail adress.
                    answer = "not found";
                }
                else
                {
                    //User is registered, info is in the DB. But we need to check if it's ADMIN or Default User.
                    if (usr.IsAdmin == true)
                    {
                        //Admin login.
                        answer = "admin";
                    }
                    else
                    {
                        //Default User login.
                        answer = usr.Email;
                    }
                }
            }

            catch (Exception)
            {

                return "ERROR !";
            }

            return answer;
        }

        public static string MD5PassConverter(string value)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] btr = Encoding.UTF8.GetBytes(value);
            btr = md5.ComputeHash(btr);

            StringBuilder sb = new StringBuilder();
            foreach (byte item in btr)
            {
                sb.Append(item.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        //USER MAIL TAKEN ?
        public bool RegisterEmailControl(User user)
        {
            User? mail = context.Users.FirstOrDefault(u => u.Email == user.Email);

            if (mail == null)
            {
                return false;
            }

            return true;
        }

        //USER REGISTER
        public bool AddUser(User user)
        {
            try
            {
                user.Active = true;
                user.IsAdmin = false;
                user.Password = MD5PassConverter(user.Password);

                context.Add(user);
                context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}
