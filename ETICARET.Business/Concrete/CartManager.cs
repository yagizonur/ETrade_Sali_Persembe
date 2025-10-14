﻿using ETICARET.Business.Abstract;
using ETICARET.DataAccess.Abstract;
using ETICARET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.Business.Concrete
{
    public class CartManager : ICartService
    {
        private readonly ICartDal _cartDal;

        public CartManager(ICartDal cartDal)
        {
            _cartDal = cartDal;
        }

        public void AddToCart(string userId, int productId, int quantity)
        {
            var cart = GetCartByUserId(userId);

            if(cart is not null)
            {
                var index = cart.CartItems.FindIndex(x => x.ProductId == productId);

                if(index < 0)
                {
                    cart.CartItems.Add(
                        new CartItem()
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            CartId = cart.Id,
                        });
                }

                else
                {
                    cart.CartItems[index].Quantity += quantity;

                }

                _cartDal.Update(cart); //Data Access aracılığıyla sepeti günceller.
            }
        }

        public void ClearCart(string cardId)
        {
           _cartDal.ClearCart(cardId);
        }

        public void DeleteFromCart(string userId, int productId)
        {
            _cartDal.DeleteFromCart(userId, productId);
        }

        public Cart GetCartByUserId(string userId)
        {
            return _cartDal.GetCartByUserId(userId);
        }

        public void InitialCart(string userId)
        {
            _cartDal.Create(new Cart() { UserId = userId });
        }
    }
}
