using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TrustPay.Application.SubCategories.Commands.UpdateSubCategoryTitle;

namespace TrustPay.Application.SubCategories.Commands.UpdateSubCategoryCommand
{
    public class UpdateSubCategoryTitleCommandValidator : AbstractValidator<UpdateSubCategoryTitleCommand>
    {
        public UpdateSubCategoryTitleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Идентификатор подкатегории не может быть пустым.");

            RuleFor(x => x.NewTitle)
                .NotEmpty().WithMessage("Новый заголовок не может быть пустым.")
                .MaximumLength(100).WithMessage("Заголовок не должен превышать 100 символов.");
        }
    }

}
