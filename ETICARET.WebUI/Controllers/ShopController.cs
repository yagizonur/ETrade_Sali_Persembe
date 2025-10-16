using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ETICARET.WebUI.Controllers
{
    public class ShopController : Controller
    {
        private IProductService _productService;

        public ShopController(IProductService productService)
        {
            _productService = productService;
        }

        [Route("products/{category?}")]
        public IActionResult List(string category, int page = 1)
        {
            const int pageSize = 10;

            var products = new ProductListModel()
            {
                PageInfo = new PageInfo()
                {
                    TotalItems = _productService.GetCountByCategory(category),
                    ItemsPerPage = pageSize,
                    CurrentCategory = category,
                    CurrentPage = page
                },

                Products = _productService.GetProductByCategory(category, page, pageSize)

            };

            return View(products);
        }

        public IActionResult Details(int? id)
        {
            if(id is null)
            {
                return NotFound();
            }

            Product product = _productService.GetProductDetail(id.Value);

            return View(
                new ProductDetailsModel()
                {
                    Products = product,
                    Categories = product.ProductCategories.Select(x => x.Category).ToList(),
                    Comments = product.Comments
                });
        } 
    }
}
