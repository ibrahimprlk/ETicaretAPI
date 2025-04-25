using Application.ViewModels.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Products
{
    public class CreateProductValidator:AbstractValidator<VM_Create_Product>
    {
        public CreateProductValidator()
        {
            RuleFor(p=>p.Name)
                .NotEmpty()
                .NotNull()
                .WithMessage("Lütfen Ürün adını boş geçmeyiniz")
                .MaximumLength(150)
                .MinimumLength(5)
                .WithMessage("Lütfen isim aralığını 5 ile 150 karakter olarak giriniz");
            RuleFor(p => p.Stock)
                .NotEmpty()
                .NotNull()
                .WithMessage("Lütfen stock miktarını boş geçmeyiniz")
                .Must(s => s >= 0)
                .WithMessage("Lütfen stok bilgisini 0 dan büyük giriniz");
            RuleFor(p => p.Price)
               .NotEmpty()
               .NotNull()
               .WithMessage("Lütfen price miktarını boş geçmeyiniz")
               .Must(s => s >= 0)
               .WithMessage("Lütfen price bilgisini 0 dan büyük giriniz");
        }
    }
}
