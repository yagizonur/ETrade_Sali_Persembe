﻿using ETICARET.Entities;
using System.ComponentModel.DataAnnotations;

namespace ETICARET.WebUI.Models
{
    public class ProductModel
    {
        public ProductModel()
        {
            Images = new List<Image>();
        }
        public int Id { get; set; }

        [Required]
        [StringLength(60,MinimumLength = 2,ErrorMessage = "Ürün ismi en az 2 karakter en fazla 60 karakter olmalıdır.")]
        public string Name { get; set; }

        [Required]
        [StringLength(60,MinimumLength = 5, ErrorMessage = "Ürün açıklaması en az 5 karakter en fazla 60 karakter olmalıdır.")]
        public string Description { get; set; } 

        public List<Image>? Images { get; set; }

        [Required]
        public decimal Price { get; set; }

        public List<Category> SelectedCategories { get; set; }

        public string CategoryId { get; set; }
    }
}
