using AspNetCore_WebAPP_MVC_PROJE.Models.DbSets;
using AspNetCore_WebAPP_MVC_PROJE.Models.MVVM;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using PagedList.Core;
using Microsoft.AspNetCore.Http;

namespace AspNetCore_WebAPP_MVC_PROJE.Controllers
{
    public class HomeController : Controller
    {
        KayaliContext context = new KayaliContext();
        MainPageModel mpm = new MainPageModel();
        cls_Product cp = new cls_Product();
        cls_Order co = new cls_Order();
        cls_User cu = new cls_User();

        int mainpageCount = 0;

        public HomeController()
        {
            this.mainpageCount = context.Settings.FirstOrDefault(s => s.SettingID == 1).mainpageCount;
        }

        //Any single product.
        public IActionResult ProductDetails(int id)
        {
            cp.Highlighted_Increase(id);
            var prdDetails = cp.ProductDetails(id);
            return View(prdDetails);
        }

        #region MainPageModel methods for Index Page
        public IActionResult Index()
        {
            mpm.SliderProducts = cp.ProductSelect("Slider", mainpageCount, "", 0);
            mpm.ProductOfTheDay = cp.SpecialProductDetails("ProductOfTheDay");//kontrol paneline günün ürünü belirlemek için input koymak lazım.
            mpm.NewProducts = cp.ProductSelect("New", mainpageCount, "", 0);
            mpm.SpecialProducts = cp.ProductSelect("Special", mainpageCount, "", 0);
            mpm.DiscountProducts = cp.ProductSelect("Discount", mainpageCount, "", 0);
            mpm.HighlightedProducts = cp.ProductSelect("Highlighted", mainpageCount, "", 0);
            mpm.TopSellerProducts = cp.ProductSelect("TopSeller", mainpageCount, "", 0);
            mpm.StarredProducts = cp.ProductSelect("Starred", mainpageCount, "", 0);
            mpm.OpportunityProducts = cp.ProductSelect("Opportunity", mainpageCount, "", 0);
            mpm.AttentionedProducts = cp.ProductSelect("Attentioned", mainpageCount, "", 0);
            return View(mpm);
        }
        #endregion

        #region NEW PRODUCTS AREA
        public IActionResult NewProducts()
        {
            mpm.NewProducts = cp.ProductSelect("New", mainpageCount, "New", 0);
            return View(mpm);
        }


        public PartialViewResult _PartialNewProducts(string pageno)
        {
            int pageNumber = Convert.ToInt32(pageno);
            mpm.NewProducts = cp.ProductSelect("New", mainpageCount, "New", pageNumber);
            return PartialView(mpm);
        }
        #endregion

        #region SPECIAL PRODUCTS AREA
        public IActionResult SpecialProducts()
        {
            mpm.SpecialProducts = cp.ProductSelect("Special", mainpageCount, "Special", 0);
            return View(mpm);
        }

        public PartialViewResult _PartialSpecialProducts(string pageno)
        {
            int pageNumber = Convert.ToInt32(pageno);
            mpm.SpecialProducts = cp.ProductSelect("Special", mainpageCount, "Special", pageNumber);
            return PartialView(mpm);
        }
        #endregion

        #region DISCOUNT PRODUCTS AREA
        public IActionResult DiscountProducts()
        {
            mpm.DiscountProducts = cp.ProductSelect("Discount", mainpageCount, "Discount", 0);
            return View(mpm);
        }

        public PartialViewResult _PartialDiscountProducts(string pageno)
        {
            int pagenumber = Convert.ToInt32(pageno);
            mpm.DiscountProducts = cp.ProductSelect("Discount", mainpageCount, "Discount", pagenumber);
            return PartialView(mpm);
        }
        #endregion

        #region HIGHLIGHTED PRODUCTS AREA
        public IActionResult HighlightedProducts()
        {
            mpm.HighlightedProducts = cp.ProductSelect("Highlighted", mainpageCount, "Highlighted", 0);
            return View(mpm);
        }

        public PartialViewResult _PartialHighlightedProducts(string pageno)
        {
            int pagenumber = Convert.ToInt32(pageno);
            mpm.HighlightedProducts = cp.ProductSelect("Highlighted", mainpageCount, "Highlighted", pagenumber);
            return PartialView(mpm);
        }
        #endregion

        #region TOP SELLER PRODUCTS AREA
        //NuGet - pagedlist.core & pagedlist.core.mvc
        public IActionResult TopSellerProducts(int page = 1, int pageSize = 4)
        {
            PagedList<Product> model = new PagedList<Product>(context.Products.OrderByDescending(p => p.TopSeller), page, pageSize);
            return View("TopSellerProducts", model); //sadece model yazsak da olur.
        }
        #endregion

        #region CATEGORY - SUPPLIER PAGES
        public IActionResult CategoryPage(int id)
        {
            List<Product> products = cp.ProductSelectByCatID(id).OrderBy(p => p.ProductName).ToList();
            return View(products);
        }

        public IActionResult SupplierPage(int id)
        {
            List<Product> productsOfSupplier = cp.ProductSelectBySupID(id).OrderBy(p => p.ProductName).ToList();
            return View(productsOfSupplier);
        }
        #endregion

        #region -- CART --
        //NuGet - microsoft.aspnet.core.http
        public IActionResult CartProcess(int id)
        {
            #region explanation
            //we will keep products and quantities like:
            //  10=1&20=3&30=7...
            // ProductID=Qantity&ProductID=Qantity&ProductID=Qantity...
            #endregion


            cp.Highlighted_Increase(id);

            co.ProductID = id;
            co.Quantity = 1;

            var cookieOptions = new CookieOptions();
            var cookie = Request.Cookies["myCart"]; //cookie read

            if (cookie == null)
            {
                cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(1); //cookies will stay max 1 day along.
                cookieOptions.Path = "/";

                co.MyCart = "";
                co.AddtoMyCart(id.ToString());

                Response.Cookies.Append("myCart", co.MyCart, cookieOptions); //cookies are sending to browser
            }

            else //if the Cart is not empty
            {
                co.MyCart = cookie; //Cart info that browser has, appended to MyCart property.
                if (co.AddtoMyCart(id.ToString()) == false) //Product had not been added to Cart. Let's add it.
                {
                    Response.Cookies.Append("myCart", co.MyCart, cookieOptions);
                    cookieOptions.Expires = DateTime.Now.AddDays(1);
                    HttpContext.Session.SetString("Message", "The Product has been added to Shopping Cart.");
                    TempData["Message"] = "The Product has been added to Shopping Cart.";
                }

                else
                {
                    HttpContext.Session.SetString("Message", "You already have this Product in your Shopping Cart. The quantity can be changed in Shopping Cart page.");
                    HttpContext.Session.GetString("Message");
                    TempData["Message"] = "You already have this Product in your Shopping Cart. The quantity can be changed in Shopping Cart page.";
                }
            }
            string url = Request.Headers["Referer"].ToString();
            return Redirect(url);
        }

        public IActionResult Cart()
        {
            List<cls_Order> cart;

            if (HttpContext.Request.Query["scid"].ToString() != "")
            {
                //after clicking DELETE BUTTON in the cart page.
                string? scid = HttpContext.Request.Query["scid"];

                co.MyCart = Request.Cookies["myCart"]; //assigns the values in the cookies to the property.
                co.DeleteFromMyCart(scid); //then triggers the deleting method

                var cookieOptions = new CookieOptions();
                Response.Cookies.Append("myCart", co.MyCart, cookieOptions); //sends the property with the new values to the browser cookie.
                //Now our Shopping Cart is renewed.
                cookieOptions.Expires = DateTime.Now.AddDays(1);
                TempData["Message"] = "The Product has been REMOVED from Shopping Cart.";

                //Now we should send the last updated data to the cshtml page:
                cart = co.SelectMyCart();
                ViewBag.MyCart = cart;
                ViewBag.Cart_Table_Details = cart;
            }

            else
            {
                //after clicking CART HREF on top right corner.
                var cookie = Request.Cookies["MyCart"];
                if (cookie == null)
                {
                    co.MyCart = "";
                    cart = co.SelectMyCart();
                    ViewBag.MyCart = cart;
                    ViewBag.Cart_Table_Details = cart;
                }
                else
                {
                    var cookieOptions = new CookieOptions();
                    co.MyCart = Request.Cookies["myCart"];
                    cart = co.SelectMyCart();
                    ViewBag.MyCart = cart;
                    ViewBag.Cart_Table_Details = cart;
                }
            }

            if (cart.Count == 0)
            {
                ViewBag.MyCart = null;
            }

            return View();
        }
        #endregion
        
        #region ORDER - PROCEED CHECKOUT
        public IActionResult Order()
        {
            //HttpContext.Session.SetString("Session", "deneme");
            //HttpContext.Session.GetString("Email");

            if (HttpContext.Session.GetString("UserInfo") != null)
            {
                //Session had already started. Get the user info.
                User? usr = cu.SelectUserInfo(HttpContext.Session.GetString("UserInfo"));
                return View();
            }
            else
            {
                //Session had not started yet. Need to login and start session.
                return RedirectToAction(nameof(Login));
            }
        }
        #endregion

        #region USER LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            string answer = cu.MemberControl(user);

            if (answer == "not found")
            {
                TempData["Message"] = "Message\",\"Email or Password is not correct !";

                return View();
            }

            else if (answer == "admin")
            {
                HttpContext.Session.SetString("UserInfo", answer);
                HttpContext.Session.SetString("Admin", answer);

                return RedirectToAction("GeneralIndex","Admin");
            }

            else
            {
                HttpContext.Session.SetString("UserInfo",answer);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region USER REGISTER
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            return View();
        }
        #endregion
    }
}