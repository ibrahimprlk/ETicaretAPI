using ETicaretAPI.Application.Features.Commands.Product.CreateProduct;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Application.Validators.Products
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommandRequest>
    {
        public CreateProductValidator()
        {
            RuleFor(p => p.Name).NotEmpty().NotNull().WithMessage("Lütfen ürün adını boş bırakmayınız").MaximumLength(150).MinimumLength(5).WithMessage("Lütfen ürün adını 5 ile 150 karakter olsun");
            RuleFor(p => p.Stock).NotEmpty().NotNull().WithMessage("Lütfen stok bilgisini giriniz").Must(s => s >= 0).WithMessage("stok bilgisini pozitif giriniz");
            RuleFor(p => p.Price).NotEmpty().NotNull().WithMessage("Lütfen fiyat bilgisini giriniz").Must(s => s >= 0).WithMessage("fiyat bilgisini pozitif giriniz");
        }
    }
}
