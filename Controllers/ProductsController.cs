using CrudMVC_App.BLL.Services;
using CrudMVC_App.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudMVC_App.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        //private readonly IProductService _service;
        private readonly IUnitOfWork _uow;

        public ProductsController(IUnitOfWork uow)
        {
           // _service = service;
            _uow = uow;
        }
        
        //[Authorize(Roles = Roles.User)]
        public IActionResult Index()
        {
            return View(_uow.Products.GetAll());
        }
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                //_service.Create(product);
                _uow.Products.Add(product);
                _uow.Save();
                return RedirectToAction("Index");
            }
            return View(product);
        }
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Edit(int id)
        {
            return View(_uow.Products.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Edit(Product product)
        {
            //_service.Edit(product);
            _uow.Products.Update(product);
            return RedirectToAction("Index");
        }
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Delete(int id)
        {
            // _service.Remove(id);
            _uow.Products.Delete(id);
            _uow.Save();
            return RedirectToAction("Index");
        }
    }
}
