﻿using ETICARET.DataAccess.Abstract;
using ETICARET.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.DataAccess.Concrete
{
    public class EfCoreCategoryDal : EfCoreGenericRepository<Category, DataContext>, ICategoryDal
    {
        public void DeleteFromCategory(int categoryId, int productId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"DELETE FROM ProductCategory WHERE ProductId = @p1 AND CategoryId = @p0";
                context.Database.ExecuteSqlRaw(cmd, categoryId, productId);
            }
        }

        public Category GetByIdWithProducts(int id)
        {
            using(var context = new DataContext())
            {
                return context.Categories
                    .Where(i => i.Id == id)
                    .Include(i => i.ProductCategories)
                    .ThenInclude(i => i.Product)
                    .ThenInclude(i => i.Images)
                    .FirstOrDefault();
            }
        }

        public override void Delete(Category entity)
        {
            using (var context = new DataContext())
            {
                context.Categories.Remove(entity);
                context.SaveChanges();
            }
        }
    }
}
